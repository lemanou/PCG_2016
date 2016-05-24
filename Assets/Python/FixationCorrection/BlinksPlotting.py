__author__ = 'Eleftherios'
# -*- coding: utf-8 -*-
"""
Created on April 152016
PCG Thesis - Blinks Plotting
Main program.
using pandas 0.18.0
---------------------------------------------------------
"""

import csv
import datetime
import time
import numpy as np
import pandas

ewma = pandas.stats.moments.ewma
import matplotlib.pyplot as plt

input_files = {"Blinks For LoadSavedLevel1.csv", "Blinks For scene2.csv"}
path = "2016.04.21/Panos/"


def hampel_filter(secondsArray):
    # hampel filter algorithm
    my_list = []
    for value in range(0, int(secondsArray.max())):
        count = 0
        testneg = max(value - 7.5, 0)
        testpos = min(value + 7.5, secondsArray.max())
        for v in secondsArray:
            if v >= testneg and v <= testpos:
                # print "For point: ", value, " value: ", v, "found in range: ", testneg, testpos
                count += 1
            elif v > testpos:
                # print "Skipping bigger num: " , testpos
                break
        # dividing by current interval and multiplying by 60 seconds to get blinks per minute
        count = (count / (testpos - testneg)) * 60
        my_list.append(count)
    return my_list


def create_count_list(a_list):
    count = 0
    count_list = []
    for i in a_list:
        count += 1
        count_list.append(count)
    return count_list


def calculate_ewma(a_list):
    fwd = ewma(a_list, span=15)  # take EWMA in fwd direction
    bwd = ewma(a_list[::-1], span=15)  # take EWMA in bwd direction
    c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
    c = np.mean(c, axis=0)  # average
    return c


def weighted_moving_average(inputfile, arrayForNormalBlinksSeconds, arrayForLongBlinkSeconds=None):
    blink_type = "AllBlinks"
    if arrayForLongBlinkSeconds is None:
        # Make ~ All blinks plot
        mySecondsArray = np.array(arrayForNormalBlinksSeconds)
        my_normalList = np.array(hampel_filter(mySecondsArray))
        plt.plot(my_normalList, alpha=0.2, label='Raw')

        # take EWMA in both directions with a smaller span term
        c = calculate_ewma(my_normalList)
        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')
        # "corrected" (?) EWMA
        plt.plot(c, 'r', label='Reversed-Recombined')

        # Calculate linear regression
        countList = create_count_list(my_normalList)
        z = np.polyfit(countList, my_normalList, 1)
        p = np.poly1d(z)
        plt.plot(countList, p(countList), 'r--', alpha=0.7, label='Linear Regression')
    else:
        # Make ~ Normal against Long blinks plot
        blink_type = "NormVsLong"
        mySecondsArray = np.array(arrayForNormalBlinksSeconds)
        my_normalList = np.array(hampel_filter(mySecondsArray))
        myLongSecondsArray = np.array(arrayForLongBlinkSeconds)
        my_longList = np.array(hampel_filter(myLongSecondsArray))

        # take EWMA in both directions with a smaller span term
        c = calculate_ewma(my_normalList)
        clong = calculate_ewma(my_longList)
        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')
        # "corrected" (?) EWMA
        plt.plot(c, 'r', label='Recombined EWMA Normal')
        plt.plot(clong, 'b', label='Recombined EWMA Long')

        # Calculate linear regression
        cnt_list = create_count_list(my_normalList)
        z = np.polyfit(cnt_list, my_normalList, 1)
        p = np.poly1d(z)
        plt.plot(cnt_list, p(cnt_list), 'r--', alpha=0.7, label='Linear Regression Normal')

        cnt_list = create_count_list(my_longList)
        z = np.polyfit(cnt_list, my_longList, 1)
        p = np.poly1d(z)
        plt.plot(cnt_list, p(cnt_list), 'b--', alpha=0.7, label='Linear Regression Long')

    plt.xlabel('Seconds')
    plt.ylabel('Blinks per minute')
    # plt.legend(loc=1)
    # Put a legend to the right of the current axis
    plt.legend(loc='lower left', bbox_to_anchor=(0.55, 0.85))
    plt.savefig(path + 'ewma_' + inputfile[:-4] + ' ' + blink_type + '.png', fmt='png', dpi=100)  # correctionOverTime
    plt.show()


def normal_against_long(input_file):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    once_normal = True
    once_long = True

    starttime = 0.0
    list_of_datetimesN = []
    list_of_datetimesL = []
    for row in reader:

        timeminutes = (row['TimeTracker'][11:]).split('.')[0]
        timemiliseconds = (row['TimeTracker'][11:]).split('.')[1]
        timemiliseconds = float(timemiliseconds) / 1000
        x = time.strptime(timeminutes, '%H:%M:%S')

        if row['BlinkType'] == "NormalBlink":
            if once_normal:
                starttime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                starttime += timemiliseconds
                list_of_datetimesN.append(0)
                once_normal = False
                continue

            newtime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            newtime -= starttime
            newtime += timemiliseconds
            list_of_datetimesN.append(newtime)
        elif row['BlinkType'] == "LongBlink":
            if once_long:
                starttime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                starttime += timemiliseconds
                list_of_datetimesN.append(0)
                once_long = False
                continue

            newtime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            newtime -= starttime
            newtime += timemiliseconds
            list_of_datetimesL.append(newtime)

    file1.close()

    if len(list_of_datetimesL) > 0:
        weighted_moving_average(input_file, list_of_datetimesN, list_of_datetimesL)


def all_blinks(input_file):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    once = True

    starttime = 0.0
    list_of_datetimes = []
    for row in reader:
        # print row['BlinkType']
        timeminutes = (row['TimeTracker'][11:]).split('.')[0]
        timemiliseconds = (row['TimeTracker'][11:]).split('.')[1]
        timemiliseconds = float(timemiliseconds) / 1000
        x = time.strptime(timeminutes, '%H:%M:%S')
        if once:
            starttime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            starttime += timemiliseconds
            list_of_datetimes.append(0)
            once = False
            continue

        newtime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
        newtime -= starttime
        newtime += timemiliseconds
        list_of_datetimes.append(newtime)

    file1.close()
    # print list_of_datetimes
    # wma_jaggy_notused(list_of_datetimes)

    if len(list_of_datetimes) > 0:
        weighted_moving_average(input_file, list_of_datetimes)


for tmp_file in input_files:
    all_blinks(tmp_file)
    normal_against_long(tmp_file)

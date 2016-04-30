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

input_files = {"Blinks For LoadSavedLevel2.csv", "Blinks For scene1.csv"}
path = "2016.04.28/Guy2/"
type = "AllBlinks"


# def wma_jaggy_notused(secondsArray):
#     mySecondsArray = np.array(secondsArray)
#
#     my_list = []
#
#     for value in mySecondsArray:
#         count = 0
#         testneg = max(value - 7.5, 0)
#         testpos = min(value + 7.5, mySecondsArray.max())
#         for v in mySecondsArray:
#             if v >= testneg and v <= testpos:
#                 # print "For point: ", value, " value: ", v, "found in range: ", testneg, testpos
#                 count += 1
#             elif v > testpos:
#                 # print "Skipping bigger num: " , testpos
#                 break
#         count = (count / (testpos - testneg)) * 60
#         my_list.append(count)
#
#     plt.plot(mySecondsArray, my_list, alpha=0.4, label='Raw')
#
#     # take EWMA in both directions with a smaller span term
#     fwd = ewma(mySecondsArray, span=15)  # take EWMA in fwd direction
#     bwd = ewma(mySecondsArray[::-1], span=15)  # take EWMA in bwd direction
#     c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
#     c = np.mean(c, axis=0)  # average
#
#     # regular EWMA, with bias against trend
#     plt.plot(fwd, my_list, 'b', label='EWMA, span=15')
#
#     # "corrected" (?) EWMA
#     plt.plot(c, my_list, 'r', label='Reversed-Recombined')
#
#     # plt.legend(loc=1)
#     # Put a legend to the right of the current axis
#     plt.legend(loc='lower center', bbox_to_anchor=(0.5, 1))
#     plt.savefig('test.png', fmt='png', dpi=100)
#     plt.show()


def weighted_moving_average(secondsArray, inputfile, arrayForLongBlinkSeconds=None):
    mySecondsArray = np.array(secondsArray)
    my_list = []

    if arrayForLongBlinkSeconds is None:
        # Make all blinks plot
        for value in range(0, int(mySecondsArray.max())):
            count = 0
            testneg = max(value - 7.5, 0)
            testpos = min(value + 7.5, mySecondsArray.max())
            for v in mySecondsArray:
                if v >= testneg and v <= testpos:
                    # print "For point: ", value, " value: ", v, "found in range: ", testneg, testpos
                    count += 1
                elif v > testpos:
                    # print "Skipping bigger num: " , testpos
                    break
            count = (count / (testpos - testneg)) * 60
            my_list.append(count)

        # print my_list, len(my_list)
        my_list = np.array(my_list)
        plt.plot(my_list, alpha=0.2, label='Raw')

        # take EWMA in both directions with a smaller span term
        fwd = ewma(my_list, span=15)  # take EWMA in fwd direction
        bwd = ewma(my_list[::-1], span=15)  # take EWMA in bwd direction
        c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
        c = np.mean(c, axis=0)  # average

        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')

        # "corrected" (?) EWMA
        plt.plot(c, 'r', label='Reversed-Recombined')

        plt.xlabel('Seconds')
        plt.ylabel('Blinks per minute')
        # plt.legend(loc=1)
        # Put a legend to the right of the current axis
        plt.legend(loc='lower center', bbox_to_anchor=(0.5, 0.97))
        plt.savefig(path + 'ewma_' + inputfile + ' ' + type + '.png', fmt='png', dpi=100)  # correctionOverTime
        plt.show()
    else:
        # Make all normal against long blinks plot
        myLongSecondsArray = np.array(arrayForLongBlinkSeconds)
        my_Longlist = []
        for value in range(0, int(myLongSecondsArray.max())):
            count = 0
            testneg = max(value - 7.5, 0)
            testpos = min(value + 7.5, myLongSecondsArray.max())
            for v in myLongSecondsArray:
                if v >= testneg and v <= testpos:
                    # print "For point: ", value, " value: ", v, "found in range: ", testneg, testpos
                    count += 1
                elif v > testpos:
                    # print "Skipping bigger num: " , testpos
                    break
            count = (count / (testpos - testneg)) * 60
            my_Longlist.append(count)

        for value in range(0, int(mySecondsArray.max())):
            count = 0
            testneg = max(value - 7.5, 0)
            testpos = min(value + 7.5, mySecondsArray.max())
            for v in mySecondsArray:
                if v >= testneg and v <= testpos:
                    # print "For point: ", value, " value: ", v, "found in range: ", testneg, testpos
                    count += 1
                elif v > testpos:
                    # print "Skipping bigger num: " , testpos
                    break
            count = (count / (testpos - testneg)) * 60
            my_list.append(count)

        # print my_list, len(my_list)
        my_list = np.array(my_list)
        my_Longlist = np.array(my_Longlist)
        # plt.plot(my_list, alpha=0.2, label='Raw')

        # take EWMA in both directions with a smaller span term
        fwd = ewma(my_list, span=15)  # take EWMA in fwd direction
        bwd = ewma(my_list[::-1], span=15)  # take EWMA in bwd direction
        c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
        c = np.mean(c, axis=0)  # average

        # take EWMA in both directions with a smaller span term
        fwdlong = ewma(my_Longlist, span=15)  # take EWMA in fwd direction
        bwdlong = ewma(my_Longlist[::-1], span=15)  # take EWMA in bwd direction
        clong = np.vstack((fwdlong, bwdlong[::-1]))  # lump fwd and bwd together
        clong = np.mean(clong, axis=0)  # average

        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')

        # "corrected" (?) EWMA
        plt.plot(c, 'r', label='Recombined EWMA Normal')
        plt.plot(clong, 'b', label='Recombined EWMA Long')

        plt.xlabel('Seconds')
        plt.ylabel('Blinks per minute')
        # plt.legend(loc=1)
        # Put a legend to the right of the current axis
        plt.legend(loc='lower center', bbox_to_anchor=(0.5, 0.97))
        plt.savefig(path + 'ewma_' + inputfile + ' ' + 'NormVsLong' + '.png', fmt='png', dpi=100)  # correctionOverTime
        plt.show()


def plotNormalAgainstLong(inputfile):
    file1 = open(path + inputfile, 'rb')
    reader = csv.DictReader(file1)
    onceN = True
    onceL = True

    starttime = 0.0
    list_of_datetimesN = []
    list_of_datetimesL = []
    for row in reader:

        timeminutes = (row['TimeTracker'][11:]).split('.')[0]
        timemiliseconds = (row['TimeTracker'][11:]).split('.')[1]
        timemiliseconds = float(timemiliseconds) / 1000
        x = time.strptime(timeminutes, '%H:%M:%S')

        if row['BlinkType'] == "NormalBlink":
            if onceN:
                starttime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                starttime += timemiliseconds
                list_of_datetimesN.append(0)
                onceN = False
                continue

            newtime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            newtime -= starttime
            newtime += timemiliseconds
            list_of_datetimesN.append(newtime)
        elif row['BlinkType'] == "LongBlink":
            if onceL:
                starttime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                starttime += timemiliseconds
                list_of_datetimesN.append(0)
                onceL = False
                continue

            newtime = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            newtime -= starttime
            newtime += timemiliseconds
            list_of_datetimesL.append(newtime)

    file1.close()

    if len(list_of_datetimesL) > 0:
        weighted_moving_average(list_of_datetimesN, inputfile[:-4], list_of_datetimesL)


def main(inputfile):
    file1 = open(path + inputfile, 'rb')
    reader = csv.DictReader(file1)
    once = True

    starttime = 0.0
    list_of_datetimes = []
    for row in reader:
        # print row['BlinkType']
        if row['BlinkType'] == type or type == "AllBlinks":
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
        weighted_moving_average(list_of_datetimes, inputfile)


for tmpfile in input_files:
    main(tmpfile)
    plotNormalAgainstLong(tmpfile)

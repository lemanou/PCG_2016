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

inputfiles = {"BlinksLoadedSigurdBlue.csv", "BlinksSceneSigurdBlue.csv", "BlinksLoadedElefBrown.csv",
              "BlinksSceneElefBrown.csv"}


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


def weighted_moving_average(secondsArray, inputfile):
    mySecondsArray = np.array(secondsArray)

    my_list = []

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
    plt.legend(loc='lower center', bbox_to_anchor=(0.5, 1))
    plt.savefig('ewma_correctionOverTime' + inputfile + '.png', fmt='png', dpi=100)
    plt.show()


def main(inputfile):
    file1 = open(inputfile, 'rb')
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
    weighted_moving_average(list_of_datetimes, inputfile)
    # wma_jaggy_notused(list_of_datetimes)


for tmpfile in inputfiles:
    main(tmpfile)

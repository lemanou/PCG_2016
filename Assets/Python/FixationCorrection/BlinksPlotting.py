__author__ = 'Eleftherios'
# -*- coding: utf-8 -*-
"""
Created on April 15 2016
PCG Thesis - Blinks Plotting
using pandas 0.18.0
---------------------------------------------------------
"""
import csv
import datetime
import time
import numpy as np
import matplotlib.pyplot as plt
import pandas

ewma = pandas.stats.moments.ewma
input_files = {"Blinks For LoadSavedLevel1.csv", "Blinks For scene2.csv"}
path = "2016.05.13/Girl3/"


def hampel_filter(seconds_array):
    # hampel filter algorithm
    my_list = []
    for value in range(0, int(seconds_array.max())):
        count = 0
        test_neg = max(value - 7.5, 0)
        test_pos = min(value + 7.5, seconds_array.max())
        for v in seconds_array:
            if test_neg <= v <= test_pos:
                # print "For point: ", value, " value: ", v, "found in range: ", test_neg, test_pos
                count += 1
            elif v > test_pos:
                # print "Skipping bigger num: " , test_pos
                break
        # dividing by current interval and multiplying by 60 seconds to get blinks per minute
        count = (count / (test_pos - test_neg)) * 60
        my_list.append(count)
    return my_list


def create_count_list(a_list):
    count_list = []
    for i in range(0, len(a_list)):
        count_list.append(i)
    return count_list


def calculate_ewma(a_list):
    fwd = ewma(a_list, span=15)  # take EWMA in fwd direction
    bwd = ewma(a_list[::-1], span=15)  # take EWMA in bwd direction
    c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
    c = np.mean(c, axis=0)  # average
    return c


def calculate_needed_stuff(array_blinks_seconds, polynomial_degree):
    np_array_blinks = np.array(array_blinks_seconds)
    list_blinks = np.array(hampel_filter(np_array_blinks))
    # take EWMA in both directions with a smaller span term
    c = calculate_ewma(list_blinks)
    # Calculate linear regression
    cnt_list = create_count_list(list_blinks)
    # calculate polynomial
    z = np.polyfit(cnt_list, list_blinks, polynomial_degree)
    p = np.poly1d(z)
    return list_blinks, c, cnt_list, p


def weighted_moving_average(input_file, array_normal_blinks_seconds, array_long_blinks_seconds=None):
    polynomial_degree = 2
    blink_type = "AllBlinks"
    list_normal_blinks, c, cnt_list, p = calculate_needed_stuff(array_normal_blinks_seconds, polynomial_degree)
    # regular EWMA, with bias against trend
    # plt.plot(fwd, 'b', label='EWMA, span=15')
    # "corrected" (?) EWMA
    plt.plot(c, 'r', label='Reversed-Recombined')
    plt.plot(cnt_list, p(cnt_list), 'r--', alpha=0.7, label='Linear Regression')

    if array_long_blinks_seconds is None:
        # Make ~ All blinks plot
        plt.plot(list_normal_blinks, alpha=0.2, label='Raw')
    else:
        # Make ~ Normal against Long blinks plot
        blink_type = "NormVsLong"
        list_long_blinks, c_long, cnt_list_long, pl = calculate_needed_stuff(array_long_blinks_seconds,
                                                                             polynomial_degree)
        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')
        # "corrected" (?) EWMA
        plt.plot(c_long, 'b', label='Recombined EWMA Long')
        plt.plot(cnt_list_long, pl(cnt_list_long), 'b--', alpha=0.7, label='Linear Regression Long')

    plt.xlabel('Seconds')
    plt.ylabel('Blinks per minute')
    # plt.legend(loc=1)
    # Put a legend to the right of the current axis
    plt.legend(loc='lower left', bbox_to_anchor=(0.55, 0.85))
    plt.savefig(path + 'ewma_p' + str(polynomial_degree) + ' ' + input_file[:-4] + ' ' + blink_type + '.png', fmt='png',
                dpi=100)  # correctionOverTime
    plt.show()

    # calculate Explained Variance
    from sklearn import linear_model, metrics
    from scipy import stats

    clf = linear_model.LinearRegression()
    # print list_normal_blinks, type(list_normal_blinks)
    # print p(cnt_list), type(p(cnt_list))
    # print x.shape, y.shape
    # x = np.linspace(0, 1, len(y))
    cnt_list = np.array(cnt_list)
    x = cnt_list.flatten().reshape(-1, 1)
    y = p(cnt_list).flatten().reshape(-1, 1)
    print("Polynomial level:%f" % polynomial_degree)
    print("x_min: %f x_max: %f" % (np.amin(x), np.amax(x)))
    clf.fit(x, y)

    rss = np.sum((clf.predict(x) - y) ** 2)
    var = np.var(y)
    sos = var * len(x)
    print "y = %fx + %f" % (clf.coef_, clf.intercept_)
    print("Residual sum of squares: %f" % rss)
    print("Original variance: %f and Sum of Squares: %f" % (var, sos))
    print("Explained variance score: %f" % clf.score(x, y))
    print("Explained variance score (different way to get same result): %f" % (1. - rss / sos))

    print("Remaining mean absolute error: %f" % metrics.mean_absolute_error(y, clf.predict(x)))
    print("Pearson Correlation R: %f p: %f" % stats.pearsonr(x, y))
    plt.plot(x, clf.predict(x))
    plt.show()


def normal_against_long(input_file):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    once_normal = True
    once_long = True

    start_time = 0.0
    list_datetimes_normal = []
    list_datetimes_long = []
    for row in reader:

        time_minutes = (row['TimeTracker'][11:]).split('.')[0]
        time_milliseconds = (row['TimeTracker'][11:]).split('.')[1]
        time_milliseconds = float(time_milliseconds) / 1000
        x = time.strptime(time_minutes, '%H:%M:%S')

        if row['BlinkType'] == "NormalBlink":
            if once_normal:
                start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                start_time += time_milliseconds
                list_datetimes_normal.append(0)
                once_normal = False
                continue

            new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            new_time -= start_time
            new_time += time_milliseconds
            list_datetimes_normal.append(new_time)
        elif row['BlinkType'] == "LongBlink":
            if once_long:
                start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                start_time += time_milliseconds
                list_datetimes_normal.append(0)
                once_long = False
                continue

            new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            new_time -= start_time
            new_time += time_milliseconds
            list_datetimes_long.append(new_time)

    file1.close()

    if len(list_datetimes_long) > 0:
        weighted_moving_average(input_file, list_datetimes_normal, list_datetimes_long)


def all_blinks(input_file):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    once = True

    start_time = 0.0
    list_of_datetimes = []
    for row in reader:
        # print row['BlinkType']
        time_minutes = (row['TimeTracker'][11:]).split('.')[0]
        time_milliseconds = (row['TimeTracker'][11:]).split('.')[1]
        time_milliseconds = float(time_milliseconds) / 1000
        x = time.strptime(time_minutes, '%H:%M:%S')
        if once:
            start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            start_time += time_milliseconds
            list_of_datetimes.append(0)
            once = False
            continue

        new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
        new_time -= start_time
        new_time += time_milliseconds
        list_of_datetimes.append(new_time)

    file1.close()

    if len(list_of_datetimes) > 0:
        weighted_moving_average(input_file, list_of_datetimes)


for tmp_file in input_files:
    all_blinks(tmp_file)
    normal_against_long(tmp_file)

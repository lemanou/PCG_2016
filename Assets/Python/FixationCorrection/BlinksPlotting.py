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
from sklearn import metrics
from scipy import stats
from matplotlib.font_manager import FontProperties

ewma = pandas.stats.moments.ewma
polynomial_degree = 2
window_span = 60


def reject_outliers(data, m=3):
    # if the deviation of each data point from the mean is less than the 3 standard deviations of the data, then keep it
    # ddof = 1 since we are using a values sample and not values of the whole session we use Bessel's correction
    return data[abs(data - np.mean(data)) < m * np.std(data, ddof=1)]


def hampel_filter(seconds_array):
    # hampel filter algorithm
    window = window_span / 2.0
    data_per_minute = []

    max_in_array = int(seconds_array.max())

    for second in range(0, max_in_array):  # loop through the array for each second and not for each member
        count = 0
        test_neg = max(second - window, 0)
        test_pos = min(second + window, max_in_array)
        for s in seconds_array:
            if test_neg <= s <= test_pos:
                # print "For point: ", value, " value: ", v, "found in range: ", test_neg, test_pos
                count += 1
            elif s > test_pos:
                # print "Skipping bigger num: " , test_pos
                break
        # dividing by current interval and multiplying by 60 seconds to get blinks per minute
        count = (count / (test_pos - test_neg)) * 60
        data_per_minute.append(count)

    result_data = reject_outliers(np.array(data_per_minute))

    # a = create_count_list(result_data)
    # b = create_count_list(data_per_minute)
    # plt.plot(b, data_per_minute, label='Raw')
    # plt.plot(a, result_data, label='After outliers')
    # plt.legend(loc='center right', bbox_to_anchor=(1.1, 0.0), shadow=True, title="Hampel effect")
    # plt.show()

    return result_data


def create_count_list(a_list):
    count_list = []
    for i in range(1, len(a_list) + 1):  # need to start the list contents from 1 to len +1
        count_list.append(i)
    return count_list


def calculate_ewma(a_list):
    fwd = ewma(a_list, span=window_span)  # take EWMA in fwd direction
    bwd = ewma(a_list[::-1], span=window_span)  # take EWMA in bwd direction
    c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
    c = np.mean(c, axis=0)  # average
    return c


def calculate_needed_stuff(array_blinks_seconds):
    np_array_blinks = np.array(array_blinks_seconds)
    list_blinks = np.array(hampel_filter(np_array_blinks))
    # take EWMA in both directions with a smaller span term
    c = calculate_ewma(list_blinks)
    # Calculate linear regression
    cnt_list = create_count_list(list_blinks)
    # calculate polynomial
    z2 = np.polyfit(cnt_list, list_blinks, polynomial_degree)
    p2 = np.poly1d(z2)
    z1 = np.polyfit(cnt_list, list_blinks, 1)
    p1 = np.poly1d(z1)
    return list_blinks, c, cnt_list, p2, p1


def calculate_ev_two(distance_considered, a_list):
    # calculate Explained Variance ~ not used
    from sklearn import linear_model, metrics
    from scipy import stats

    clf = linear_model.LinearRegression()
    # print list_normal_blinks, type(list_normal_blinks)
    # print p(cnt_list), type(p(cnt_list))
    # print x.shape, y.shape
    # x = np.linspace(0, 1, len(y))
    d_list = np.array(distance_considered)
    x = d_list.flatten().reshape(-1, 1)
    y = a_list.flatten().reshape(-1, 1)
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


def calculate_ev(path, distance_considered, a_list):
    ev_list = []
    d_list = np.array(distance_considered)
    x = d_list  # .flatten().reshape(-1, 1)
    y = a_list  # .flatten().reshape(-1, 1)
    # calculate sos
    var = np.var(y)
    sos = var * len(x)  # sum of squares
    plt.plot(x, y, 'o', alpha=0.7, label='Data', color='gray')
    for pd in range(1, 16):
        p = np.polyfit(x, y, pd)
        rss = np.sum((np.polyval(p, x) - y) ** 2)  # residual sos
        ev = (1. - rss / sos)  # explained variance
        ev_list.append(ev)
        plt.plot(x, np.polyval(p, x), label=pd)

        print("=================================")
        print("Polynomial degree: %f" % pd)
        print("Residual sum of squares: %f" % rss)
        print("Explained variance score: %f" % ev)
        print("Remaining mean absolute error: %f" % metrics.mean_absolute_error(y, (np.polyval(p, x))))
    print("Original variance: %f and Sum of Squares: %f" % (var, sos))
    print("Pearson Correlation R: %f p: %f" % stats.pearsonr(x, y))

    font_p = FontProperties()
    font_p.set_size('small')
    plt.legend(loc='lower right', bbox_to_anchor=(1.1, 0.0), shadow=True, title="Polynomial degree", prop=font_p)
    plt.savefig(path + 'pd_p' + str(polynomial_degree) + ' ' + str(ev) + '.png', fmt='png', dpi=100)
    plt.show()

    plt.plot(create_count_list(ev_list), ev_list, label='Explained variance')
    plt.legend(loc='lower right', bbox_to_anchor=(1.1, 0.0), shadow=True, title="Explained variance", prop=font_p)
    plt.axis([0, 16, 0, 1])
    plt.savefig(path + 'ev_p' + str(polynomial_degree) + ' ' + str(ev) + '.png', fmt='png', dpi=100)
    plt.show()

    return ev_list


def calculate_ev_and_save_to_csv(input_file, path, array_normal_blinks_seconds):
    list_normal_blinks, c, cnt_list, p, p1 = calculate_needed_stuff(array_normal_blinks_seconds)

    ev_list = calculate_ev(path, cnt_list, list_normal_blinks)
    # calculate_ev_two(cnt_list, list_normal_blinks) # different way, same result

    import csv

    with open('data.csv', 'ab') as fp:
        csv.writer(fp, delimiter=',', quoting=csv.QUOTE_MINIMAL, lineterminator='\n')
        headers = ['ev', 'user']
        writer = csv.DictWriter(fp, fieldnames=headers)

        # writer.writeheader()
        for val in ev_list:
            writer.writerow({'ev': val, 'user': path + input_file[:-4]})


def weighted_moving_average(input_file, path, array_normal_blinks_seconds, array_long_blinks_seconds=None):
    blink_type = "AllBlinks"
    list_normal_blinks, c, cnt_list, p, p1 = calculate_needed_stuff(array_normal_blinks_seconds)

    # regular EWMA, with bias against trend
    # plt.plot(fwd, 'b', label='EWMA, span=15')
    # "corrected" (?) EWMA
    plt.plot(c, 'r', label='Reversed-Recombined')
    plt.plot(cnt_list, p(cnt_list), 'r--', alpha=0.7, label='Polynomial degree ' + str(polynomial_degree))

    if array_long_blinks_seconds is None:
        # Make ~ All blinks plot
        plt.plot(list_normal_blinks, alpha=0.2, label='Raw')
    else:
        # Make ~ Normal against Long blinks plot
        blink_type = "NormVsLong"
        list_long_blinks, c_long, cnt_list_long, pl, pl1 = calculate_needed_stuff(array_long_blinks_seconds)
        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')
        # "corrected" (?) EWMA
        plt.plot(c_long, 'b', label='Recombined EWMA Long')
        plt.plot(cnt_list_long, pl(cnt_list_long), 'b--', alpha=0.7,
                 label='Polynomial degree Long ' + str(polynomial_degree))

    plt.xlabel('Seconds')
    plt.ylabel('Blinks per minute')
    # plt.legend(loc=1)
    # Put a legend to the right of the current axis
    plt.legend(loc='lower left', bbox_to_anchor=(0.55, 0.85))
    plt.savefig(path + 'ewma_p' + str(polynomial_degree) + ' ' + input_file[:-4] + ' ' + blink_type + '_ws' + str(
        window_span) + '.png', fmt='png', dpi=100)  # correctionOverTime
    plt.show()

    # Same for linear this time
    plt.plot(c, 'r', label='Reversed-Recombined')
    plt.plot(cnt_list, p1(cnt_list), 'r--', alpha=0.7, label='Linear regression')

    if array_long_blinks_seconds is None:
        # Make ~ All blinks plot
        plt.plot(list_normal_blinks, alpha=0.2, label='Raw')
    else:
        # Make ~ Normal against Long blinks plot
        blink_type = "NormVsLong"
        list_long_blinks, c_long, cnt_list_long, pl, pl1 = calculate_needed_stuff(array_long_blinks_seconds)
        # regular EWMA, with bias against trend
        # plt.plot(fwd, 'b', label='EWMA, span=15')
        # "corrected" (?) EWMA
        plt.plot(c_long, 'b', label='Recombined EWMA Long')
        plt.plot(cnt_list_long, pl1(cnt_list_long), 'b--', alpha=0.7, label='Linear regression')

    plt.xlabel('Seconds')
    plt.ylabel('Blinks per minute')
    # plt.legend(loc=1)
    # Put a legend to the right of the current axis
    plt.legend(loc='lower left', bbox_to_anchor=(0.55, 0.85))
    plt.savefig(path + 'ewma_linear' + input_file[:-4] + ' ' + blink_type + '_ws' + str(
        window_span) + '.png', fmt='png', dpi=100)  # correctionOverTime
    plt.show()


def normal_against_long(input_file, path):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    once = True  # starting time set to 0 only once for one of the two lists

    start_time = 0.0
    list_datetimes_normal = []
    list_datetimes_long = []
    for row in reader:

        time_minutes = (row['TimeTracker'][11:]).split('.')[0]
        time_milliseconds = (row['TimeTracker'][11:]).split('.')[1]
        time_milliseconds = float(time_milliseconds) / 1000
        x = time.strptime(time_minutes, '%H:%M:%S')

        if row['BlinkType'] == "NormalBlink":
            if once:
                start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                start_time += time_milliseconds
                list_datetimes_normal.append(0)
                once = False
                continue

            new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            new_time -= start_time
            new_time += time_milliseconds
            list_datetimes_normal.append(new_time)
        elif row['BlinkType'] == "LongBlink":
            if once:
                start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                start_time += time_milliseconds
                list_datetimes_long.append(0)
                once = False
                continue

            new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            new_time -= start_time
            new_time += time_milliseconds
            list_datetimes_long.append(new_time)

    file1.close()

    if len(list_datetimes_long) > 1:  # don't calculate if you have less than 1 blinks
        weighted_moving_average(input_file, path, list_datetimes_normal, list_datetimes_long)


def all_blinks(input_file, plot_EVs, path):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    once = True

    start_time = 0.0
    list_of_datetimes = []
    for row in reader:
        # print row['BlinkType']
        total_time = (row['TimeTracker'][11:]).split('.')[0]
        time_milliseconds = (row['TimeTracker'][11:]).split('.')[1]
        time_milliseconds = float(time_milliseconds) / 1000
        x = time.strptime(total_time, '%H:%M:%S')
        if once:
            start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            start_time += time_milliseconds
            list_of_datetimes.append(0)
            once = False
            # print 'start time in seconds: ' + str(start_time) + ' new_time: ' + '0.000' + ' total_time: ' + str(
            #    total_time) + ' time_milliseconds: ' + str(time_milliseconds), x
            continue

        new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
        new_time -= start_time
        new_time += time_milliseconds
        list_of_datetimes.append(new_time)
        # print 'start time in seconds: ' + str(start_time) + ' new_time: ' + str(new_time) + ' total_time: ' + str(
        #    total_time) + ' time_milliseconds: ' + str(time_milliseconds), x

    file1.close()
    # print 'len of list: ' + str(len(list_of_datetimes))
    if len(list_of_datetimes) > 0:
        if plot_EVs:
            calculate_ev_and_save_to_csv(input_file, path, list_of_datetimes)
        else:
            weighted_moving_average(input_file, path, list_of_datetimes)

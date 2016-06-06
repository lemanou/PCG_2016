__author__ = 'Eleftherios'
# -*- coding: utf-8 -*-
"""
Created on Mar 27 2016
PCG Thesis - Data Clustering
---------------------------------------------------------
"""

import numpy as np
import matplotlib.pyplot as plt
import math
import random
from matplotlib.font_manager import FontProperties
import csv
import time
import datetime
import pandas

ewma = pandas.stats.moments.ewma

# import matplotlib.animation as animation
# from matplotlib.collections import LineCollection

# outfilename = "DataCluster"
# fixationradius = ""
# inputfile = "Data - ResultRaw" + str(fixationradius) + ".csv"


# def testPlotSaccadesOnly():
#     data = np.genfromtxt(inputfile, delimiter=',', skip_header=1,
#                          skip_footer=0, names=['CSX', 'CSY'])
#
#     fig = plt.figure()
#     myPlot = fig.add_subplot(111)
#
#     myPlot.set_title("Game Gaze ~ test subject 1")
#     myPlot.set_xlabel('Pixels')
#     myPlot.set_ylabel('Pixels')
#
#     myPlot.plot(data['CSX'], data['CSY'], color='r', label='the data')
#
#     # plt.savefig("plot.png")
#     plt.show()
#
#
# def testPlotWithFixations():
#     file1 = open(inputfile, 'rb')
#     reader = csv.DictReader(file1)
#
#     fixations_list = []
#     # saccades_list = []
#
#     for row in reader:
#         # if row['Result'] == 'Saccade':
#         # saccades_list.append([float(row['CSX']), float(row['CSY'])])
#         # elif
#         if row['Result'] == 'Fixation':
#             fixations_list.append([float(row['CRX']), float(row['CRY'])])
#             # else:
#             #   print 'Error in file'
#
#     file1.close()
#
#     fig = plt.figure()
#     myPlot = fig.add_subplot(111)
#     # for row in saccades_list:
#     #    myPlot.scatter(row[0],row[1], color='r', label='Saccades')
#
#     myPlot.set_title("Game Gaze ~ test subject 1")
#     myPlot.set_xlabel('Pixels')
#     myPlot.set_ylabel('Pixels')
#     myPlot.axis([0, 1600, 0, 900])
#
#     for row in fixations_list:
#         myPlot.scatter(row[0], row[1], s=5, color='b', label='Fixations', zorder=10)
#
#     data = np.genfromtxt(inputfile, delimiter=',', skip_header=1,
#                          skip_footer=0, names=['CRX', 'CRY'])
#
#     myPlot.plot(data['CRX'], data['CRY'], color='r', label='All data', zorder=1)
#     plt.savefig("plotRawWithFixations" + str(fixationradius) + ".png")
#     plt.show()
#
#
# def testthree():
#     file1 = open("Data - ResultRaw.csv", 'rb')
#     reader = csv.DictReader(file1)
#
#     fixations_list = []
#     all_list = []
#
#     for row in reader:
#         all_list.append([float(row['CRX']), float(row['CRY'])])
#         if row['Result'] == 'Fixation':
#             fixations_list.append([float(row['CRX']), float(row['CRY'])])
#
#     file1.close()
#
#     fig = plt.figure()
#     myPlot = fig.add_subplot(111)
#
#     myPlot.set_title("Game Gaze ~ test subject 1")
#     myPlot.set_xlabel('Pixels')
#     myPlot.set_ylabel('Pixels')
#     myPlot.axis([0, 1600, 0, 900])
#
#     plt.ion()
#     plt.show()
#
#     # for row in all_list:
#     myPlot.plot(all_list, color='r', label='Fixations', zorder=1)
#     fig.canvas.draw()
#
#     for row in fixations_list:
#         myPlot.scatter(row[0], row[1], s=5, color='b', label='Fixations', zorder=10)
#         plt.draw()
#
#
# def testAnimated():
#     fig = plt.figure()
#     ax1 = fig.add_subplot(1, 1, 1)
#
#     pullData = np.genfromtxt('Data - ResultRaw.csv', delimiter=',', skip_header=1,
#                              skip_footer=0, names=['CRX', 'CRY'])
#
#     plt.ion()
#     plt.show()
#
#     xar = []
#     yar = []
#     for eachLine in pullData:
#         if len(eachLine) > 1:
#             x, y = str(eachLine).split(',')
#             xar.append(float(x[1:]))
#             yar.append(float(y[:-1]))
#             print eachLine
#             ax1.clear()
#             ax1.autoscale(False, axis='both')
#             ax1.axis([0, 1600, 0, 900])
#             ax1.plot(xar, yar)
#             fig.canvas.draw()

# testPlotSaccadesOnly()
# testPlotWithFixations()
# testthree()
# testAnimated()

input_files = {"Gazes For LoadSavedLevel1.csv", "Gazes For LoadSavedLevel2.csv", "Gazes For scene3.csv"}
path = "2016.04.21/Guy1/"
# ================= region DBSCAN testing ============================== #
# Configurable values
min_fix = 0.100
min_cluster_size = 50.0
frame_res = 60.0
polynomial_degree = 1

# Derived value(s)
min_fix_pts = round(min_fix * frame_res)


def spat_dist(p1, p2):
    return math.hypot(p1[0] - p2[0], p1[1] - p2[1])


def cluster_frames_DBSCAN(npeyeframes):
    '''
    Cluster frames with a derivate of DBSCAN taking into account the time

    This DBSCAN variation uses DBSCAN in the local spatial neighbourhood with timing constraints:
    Single or a few outliers followed by additional connected points are considered noise,
    and more points can be added to the current cluster
    However if enough disjoint points are seen that could make up a cluster (temporally!) it ends the current cluster
    '''

    def query_region(frames, first, reference):
        '''returns absolute indices of frames in neighbourhood of reference, from first to break/len(frames)'''
        i = first
        region = set()
        seq_outside = 0
        while i < len(frames):
            if spat_dist(frames[reference], npeyeframes[i]) < min_cluster_size:
                region.add(i)
                seq_outside = 0
            else:
                if i > reference:
                    seq_outside += 1
                    if seq_outside >= min_fix_pts:
                        break
            i += 1
        return region

    # first create labels; 0 means unassigned (so far)
    labels = np.zeros(len(npeyeframes), dtype=int)

    label = 1
    unassigned = 0
    nextp = unassigned
    while nextp < len(labels):
        # print ("DBSCAN index %d" % nextp)
        if labels[nextp] == 0:
            neighbours = query_region(npeyeframes, unassigned, nextp)
            if len(neighbours) < min_fix_pts:
                print ("   Too few neighbours; leaving point alone: %d" % len(neighbours))
            else:
                # print ("   New cluster: %d [Initial: %d]" % (label, len(neighbours)))
                unvisited = neighbours
                unvisited.remove(nextp)
                visited = set()
                visited.add(nextp)

                while len(unvisited) > 0:
                    first = unvisited.pop()

                    visited.add(first)
                    neighbours = query_region(npeyeframes, unassigned, first)
                    if len(neighbours) >= min_fix_pts:
                        unvisited.update(neighbours - visited)

                for v in visited:
                    assert labels[v] == 0, "Reassigning point to new cluster, should not take place"
                    labels[v] = label
                    nextp = max(nextp, v)

                unassigned = nextp + 1
                label += 1
                # print ("   Next unassigned: %d [Final: %d]" % (unassigned, len(visited)))
        else:
            print ("   Already assigned to %d" % labels[nextp])

        nextp += 1

    # remove any empty last cluster
    if len(npeyeframes[labels == label]) == 0:
        # print ("   Last cluster (%d) is empty, removing" % (label))
        label -= 1

    # calculate the cluster centers...
    print ("   Ended up with %d clusters" % label)

    cluster_centers = np.empty([label + 1, 2 * 4])  # was 3*4
    for l in range(label + 1):
        # print ("   Fixing frames in  clusters %d" % l)

        myframes = npeyeframes[labels == l]
        mylen = len(myframes)
        if (mylen > 0):
            cmean = np.mean(myframes, 0)
            cmin = np.amin(myframes, 0)
            cmax = np.amax(myframes, 0)
            cstd = np.std(myframes, 0)
            # print  "LB:", "\n", cluster_centers[l], "\n", np.concatenate((cmean, cmin, cmax, cstd))
            cluster_centers[l] = np.concatenate((cmean, cmin, cmax, cstd))
            # print  "LA:", "\n", cluster_centers[l], "\n", len(np.concatenate((cmean, cmin, cmax, cstd)))

    return labels, cluster_centers


def map_float_list(a_list):
    return map(float, a_list)


def generate_color(r, g, b):
    r += 80
    if r > 240:
        r = 0
        g += 80
    if g > 240:
        g = 0
        b += 80
    if b > 240:
        b = 0

    # color = '#{:02x}{:02x}{:02x}'.format(r, g, b)
    color = '#{:02x}{:02x}{:02x}'.format(*map(lambda x: random.randint(0, 255), range(3)))
    return color, r, g, b


def hampel_filter_with_value(times_array, length_array):
    # hampel filter algorithm
    window = 7.5
    my_list = []
    max_in_array = times_array[-1]
    limit = len(times_array)
    if len(times_array) > len(length_array):
        print "Arrays not equal size, weird... Needs rechecking. " + str(len(times_array)) + ' > ' + str(
            len(length_array))
        print "Resetting limit"
        limit = len(length_array) - 1
    elif len(times_array) < len(length_array):
        print "Arrays not equal size, super weird... Needs rechecking. " + str(len(times_array)) + ' < ' \
              + str(len(length_array))
        print "No change."

    for second in range(0, int(max_in_array[1])):  # loop through the array for each second and not for each member
        duration = 0
        counter = 0
        # print value[1], max_in_array[1]
        test_neg = max(second - window, 0)
        test_pos = min(second + window, max_in_array[1])
        for i in range(0, limit):
            # print times_array[i][1]
            if test_neg <= times_array[i][1] <= test_pos:
                # print "For point: ", value, " value: ", v, "found in range: ", test_neg, test_pos
                duration += length_array[i]
                counter += 1
            elif times_array[i][1] > test_pos:
                # print "Skipping bigger num: " , test_pos
                break
        # average duration per fixation
        if counter > 10:
            duration /= counter
        # dividing by current interval and multiplying by 60 seconds to get fixations per minute
        duration = (duration / (test_pos - test_neg)) * 60
        my_list.append(duration)
    return my_list


def hampel_filter(seconds_array):
    # hampel filter algorithm
    window = 7.5
    my_list = []

    max_in_array = seconds_array[-1]
    for second in range(0, int(max_in_array[1])):  # loop through the array for each second and not for each member
        count = 0
        test_neg = max(second - window, 0)  # value[1] = seconds
        test_pos = min(second + window, max_in_array[1])  # max_in_array[1] = max seconds
        for v in seconds_array:
            if test_neg <= v[1] <= test_pos:
                # print "For point: ", value, " value: ", v, "found in range: ", test_neg, test_pos
                count += 1
            elif v[1] > test_pos:
                # print "Skipping bigger num: " , test_pos
                break
        # dividing by current interval and multiplying by 60 seconds to get fixations per minute
        count = (count / (test_pos - test_neg)) * 60
        my_list.append(count)
    return my_list


def create_count_list(a_list):
    count_list = []
    for i in range(1, len(a_list) + 1):
        count_list.append(i)
    return count_list


def calculate_ewma(a_list):
    fwd = ewma(a_list, span=15)  # take EWMA in fwd direction
    bwd = ewma(a_list[::-1], span=15)  # take EWMA in bwd direction
    c = np.vstack((fwd, bwd[::-1]))  # lump fwd and bwd together
    c = np.mean(c, axis=0)  # average
    return c


def calculate_needed_stuff(a_list, polynomial_degree):
    # take EWMA in both directions with a smaller span term
    a_list_np_array = np.array(a_list)
    c = calculate_ewma(a_list_np_array)
    cnt_list = create_count_list(a_list_np_array)
    # Calculate linear regression / polynomial
    z = np.polyfit(cnt_list, a_list_np_array, polynomial_degree)
    p = np.poly1d(z)
    return c, cnt_list, p


def check_if_head_of_cluster(x_y, a_list):
    x_y = np.array(x_y)
    for member in a_list:
        if (x_y == member).all():
            # print 'Found!'
            # print "Member: ", member, type(member)
            # print "x_y: ", x_y, type(x_y)
            return True
    return False


def get_all_fixations_with_seconds(input_file, a_list):
    file1 = open(path + input_file, 'rb')
    reader = csv.DictReader(file1)
    fixations_with_timestamps = []
    start_time = 0.0
    once = True
    for row in reader:
        x_y = [float(row['CRX']), float(row['CRY'])]
        a_bool = check_if_head_of_cluster(x_y, a_list)
        if a_bool:
            time_minutes = (row['TimeStamp'][11:]).split('.')[0]
            time_milliseconds = (row['TimeStamp'][11:]).split('.')[1]
            time_milliseconds = float(time_milliseconds) / 1000
            x = time.strptime(time_minutes, '%H:%M:%S')
            if once:
                start_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
                start_time += time_milliseconds
                fixations_with_timestamps.append([x_y, 0])
                once = False
                continue

            new_time = datetime.timedelta(hours=x.tm_hour, minutes=x.tm_min, seconds=x.tm_sec).total_seconds()
            new_time -= start_time
            new_time += time_milliseconds
            fixations_with_timestamps.append([x_y, new_time])

    file1.close()
    # print "FWT"
    # print fixations_with_timestamps
    return fixations_with_timestamps


def main(input_file):
    data = np.genfromtxt(path + input_file, delimiter=',', skip_header=1, names=['CRX', 'CRY'])
    data = np.array(map(map_float_list, data))  # refactor double string array to double float array

    # Cluster the data into and centers and labels
    labels, cluster_centers = cluster_frames_DBSCAN(data)

    labels_unique = np.unique(labels)
    n_clusters = len(labels_unique)

    assert n_clusters == len(cluster_centers) or n_clusters == len(cluster_centers) - 1, \
        "Weird, we got %d unique labels but %d clusters returned..." % (n_clusters, len(cluster_centers))

    fixations_list = []
    for k in labels_unique:
        my_members = labels == k  # if k==labels, then we have a member
        fixations_list.append(data[my_members])

    fig = plt.figure()
    my_plot = fig.add_subplot(111)

    my_plot.set_title("Raw Clustered")  # + str(fixationradius) +
    my_plot.set_xlabel('Pixels')
    my_plot.set_ylabel('Pixels')
    my_plot.axis([0, 1600, 0, 900])

    myr = 0
    myg = 80
    myb = 160
    length_of_fixations = []
    list_with_first_cluster_member = []
    for members in fixations_list:
        list_with_first_cluster_member.append(members[0])  # get first element of a fixation/cluster
        members_color, myr, myg, myb = generate_color(myr, myg, myb)
        count_points = 0
        for points in members:
            my_plot.scatter(points[0], points[1], s=5, color=members_color, label='Fixations', zorder=10)
            count_points += 1
        length_of_fixations.append(count_points)  # count all points in a fixation/cluster

    # plot saccades in Gazes plot
    new_data = np.genfromtxt(path + input_file, delimiter=',', skip_header=1, skip_footer=0, names=['CRX', 'CRY'])
    my_plot.plot(new_data['CRX'], new_data['CRY'], color='black', label='All data', zorder=1)
    plt.savefig(path + 'RawClustered' + input_file + '.png')
    plt.show()

    # Get fixations over time
    fixations_over_time = get_all_fixations_with_seconds(input_file, list_with_first_cluster_member)
    result_hampel_list = hampel_filter(np.array(fixations_over_time))
    result_ewma_list, cnt_list, p = calculate_needed_stuff(result_hampel_list, polynomial_degree)
    plt.plot(cnt_list, result_hampel_list, 'c--', alpha=0.9, label='Fixations Raw')
    plt.plot(cnt_list, result_ewma_list, 'b', label='Duration per fixation')
    plt.plot(cnt_list, p(cnt_list), 'b--', alpha=0.6, label='Linear Regression')
    plt.xlabel('Seconds')
    plt.ylabel('Fixations')
    font_p = FontProperties()
    font_p.set_size('small')
    plt.legend(loc='lower right', bbox_to_anchor=(1.1, 0.9), shadow=True, title="Fixations per minute", prop=font_p)
    plt.savefig(path + 'Fixations_Per_Minute_' + input_file[:-4] + '_' + str(len(result_ewma_list)) + '.png', fmt='png',
                dpi=100)
    plt.show()

    # Get duration of fixations over time
    lof = []
    for value in length_of_fixations:
        value *= 0.016  # 60 frames per second
        lof.append(value)
    result_hampel_list = hampel_filter_with_value(np.array(fixations_over_time), np.array(lof))
    result_ewma_list, cnt_list, p = calculate_needed_stuff(result_hampel_list, polynomial_degree)
    plt.plot(cnt_list, result_hampel_list, 'c--', alpha=0.9, label='Fixations Raw')
    plt.plot(cnt_list, result_ewma_list, 'b', label='Duration per fixation')
    plt.plot(cnt_list, p(cnt_list), 'b--', alpha=0.6, label='Linear Regression')
    plt.xlabel('Seconds')
    plt.ylabel('Duration (seconds)')
    font_p = FontProperties()
    font_p.set_size('small')
    plt.legend(loc='lower right', bbox_to_anchor=(1.1, 0.9), shadow=True, title="Duration of fixations", prop=font_p)
    plt.savefig(path + 'Duration_Of_Fixations_' + input_file[:-4] + '_' + str(len(result_ewma_list)) + '.png',
                fmt='png',
                dpi=100)
    plt.show()


# ================= endregion DBSCAN testing ======================== #


for tmp_file in input_files:
    main(tmp_file)

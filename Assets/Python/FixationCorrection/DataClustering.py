__author__ = 'Eleftherios'
# -*- coding: utf-8 -*-
"""
Created on Mar 27 2016
PCG Thesis - Data Clustering
Main program.
---------------------------------------------------------
"""

import numpy as np
import matplotlib.pyplot as plt
import math
import random
import csv
import matplotlib.animation as animation
from matplotlib.collections import LineCollection
import time

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

input_files = {"Gazes For LoadSavedLevel2.csv", "Gazes For LoadSavedLevel3.csv",
               "Gazes For scene1.csv"}
path = "2016.04.21/Girl2/"
# ================= region DBSCAN testing ============================== #
# Configurable values
min_fix = 0.100
min_cluster_size = 50.0
frame_res = 60.0

# Derived value(s)
min_fix_pts = round(min_fix * frame_res)


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

    color = '#{:02x}{:02x}{:02x}'.format(r, g, b)
    color = '#{:02x}{:02x}{:02x}'.format(*map(lambda x: random.randint(0, 255), range(3)))
    return color, r, g, b


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

    return (labels, cluster_centers)


def myFloat(myList):
    return map(float, myList)


def main(inputfile):
    data = np.genfromtxt(path + inputfile, delimiter=',', skip_header=1, names=['CRX', 'CRY'])
    data = np.array(map(myFloat, data))  # refactor double string array to double float array

    # Cluster the data into and centers and labels
    labels, cluster_centers = cluster_frames_DBSCAN(data)

    labels_unique = np.unique(labels)
    n_clusters = len(labels_unique)

    assert n_clusters == len(cluster_centers) or n_clusters == len(cluster_centers) - 1, \
        "Weird, we got %d unique labels but %d clusters returned..." % (n_clusters, len(cluster_centers))

    fixations_list = []
    for k in labels_unique:
        my_members = labels == k
        fixations_list.append(data[my_members])

    fig = plt.figure()
    myplot = fig.add_subplot(111)

    myplot.set_title("Raw Clustered")  # + str(fixationradius) +
    myplot.set_xlabel('Pixels')
    myplot.set_ylabel('Pixels')
    myplot.axis([0, 1600, 0, 900])

    myr = 0
    myg = 80
    myb = 160
    for members in fixations_list:
        memmbers_color, myr, myg, myb = generate_color(myr, myg, myb)
        for points in members:
            myplot.scatter(points[0], points[1], s=5, color=memmbers_color, label='Fixations', zorder=10)

    new_data = np.genfromtxt(path + inputfile, delimiter=',', skip_header=1, skip_footer=0, names=['CRX', 'CRY'])

    myplot.plot(new_data['CRX'], new_data['CRY'], color='black', label='All data', zorder=1)
    plt.savefig(path + 'RawClustered' + inputfile + '.png')
    plt.show()

    # plot total number of fixations and sacades
    n_groups = 1

    fig, ax = plt.subplots()

    index = np.arange(n_groups)
    bar_width = 0.35

    opacity = 0.4
    error_config = {'ecolor': '0.3'}

    # remove one fixation, for the Zero cluster
    rects1 = plt.bar(index,
                     len(fixations_list) - 1,
                     bar_width,
                     alpha=opacity,
                     color='b',
                     error_kw=error_config,
                     label='Men')

    plt.xlabel('Group')
    plt.ylabel('Sums')
    plt.title('Sum of fixations')
    plt.xticks(index + bar_width, ('F', 'S'))
    plt.legend()

    plt.tight_layout()
    # plt.savefig(path + 'FixationsSum' + inputfile + '.png')
    # plt.show()


# ================= endregion DBSCAN testing ======================== #

for tmpfile in input_files:
    main(tmpfile)

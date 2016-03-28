__author__ = 'Eleftherios'
# -*- coding: utf-8 -*-
"""
Created on Mar 27 2016
PCG Thesis - Data Clustering
Main program.
---------------------------------------------------------
"""

import csv
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.collections import LineCollection


def testOne():
    data = np.genfromtxt('Data - Result.csv', delimiter=',', skip_header=10,
                         skip_footer=10, names=['CSX', 'CSY'])

    fig = plt.figure()
    myPlot = fig.add_subplot(111)

    myPlot.set_title("Game Gaze ~ test subject 1")
    myPlot.set_xlabel('Pixels')
    myPlot.set_ylabel('Pixels')

    myPlot.plot(data['CSX'], data['CSY'], color='r', label='the data')

    plt.show()
    # plt.savefig("plot.png")


def main():
    file1 = open("Data - Result.csv", 'rb')
    reader = csv.DictReader(file1)

    fixations_list = []
    saccades_list = []
    fixations_list.append([])
    fixations_list.append([])
    saccades_list.append([])
    saccades_list.append([])

    tmpcount = 0
    for row in reader:
        if row['Result'] == 'Saccade':
            saccades_list[tmpcount].append(float(row['CSX']))
            saccades_list[tmpcount].append(float(row['CSY']))
            tmpcount += 1
        elif row['Result'] == 'Fixation':
            fixations_list.append([float(row['CSX']), float(row['CSY'])])
        else:
            print 'Error in file'

            file1.close()

            # myPlot.plot(saccades_list, color='r', label='Saccades')
            # myPlot.scatter(fixations_list, color='b', label='Fixations')

            xy = (np.random.random((1000, 2)) - 0.5).cumsum(axis=0)
            # Reshape things so that we have a sequence of:
            # [[(x0,y0),(x1,y1)],[(x0,y0),(x1,y1)],...]
            xy = xy.reshape(-1, 1, 2)
            segments = np.hstack([fixations_list[:-1], fixations_list[1:]])
            fig, ax = plt.subplots()
            coll = LineCollection(segments, cmap=plt.cm.gist_ncar)
            coll.set_array(np.random.random(xy.shape[0]))

            ax.add_collection(coll)
            ax.autoscale_view()

            plt.show()

# testOne()
main()

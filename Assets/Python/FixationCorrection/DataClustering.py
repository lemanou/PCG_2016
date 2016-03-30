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
import matplotlib.animation as animation
from matplotlib.collections import LineCollection
import time

fixationradius = 100
inputfile = "Data - ResultRaw" + str(fixationradius) +".csv"

def testPlotSaccadesOnly():
    data = np.genfromtxt('Data - Result.csv', delimiter=',', skip_header=10,
                         skip_footer=10, names=['CSX', 'CSY'])

    fig = plt.figure()
    myPlot = fig.add_subplot(111)

    myPlot.set_title("Game Gaze ~ test subject 1")
    myPlot.set_xlabel('Pixels')
    myPlot.set_ylabel('Pixels')

    myPlot.plot(data['CSX'], data['CSY'], color='r', label='the data')

    # plt.savefig("plot.png")
    plt.show()


def testPlotWithFixations():
    file1 = open(inputfile, 'rb')
    reader = csv.DictReader(file1)

    fixations_list = []
    # saccades_list = []

    for row in reader:
        # if row['Result'] == 'Saccade':
        # saccades_list.append([float(row['CSX']), float(row['CSY'])])
        # elif
        if row['Result'] == 'Fixation':
            fixations_list.append([float(row['CRX']), float(row['CRY'])])
            # else:
            #   print 'Error in file'

    file1.close()

    fig = plt.figure()
    myPlot = fig.add_subplot(111)
    # for row in saccades_list:
    #    myPlot.scatter(row[0],row[1], color='r', label='Saccades')

    myPlot.set_title("Game Gaze ~ test subject 1")
    myPlot.set_xlabel('Pixels')
    myPlot.set_ylabel('Pixels')
    myPlot.axis([0, 1600, 0, 900])

    for row in fixations_list:
        myPlot.scatter(row[0], row[1], s=5, color='b', label='Fixations', zorder=10)

    data = np.genfromtxt(inputfile, delimiter=',', skip_header=10,
                         skip_footer=10, names=['CRX', 'CRY'])

    myPlot.plot(data['CRX'], data['CRY'], color='r', label='All data', zorder=1)
    plt.savefig("plotRawWithFixations" + str(fixationradius) +".png")
    plt.show()


def testthree():
    file1 = open("Data - ResultRaw.csv", 'rb')
    reader = csv.DictReader(file1)

    fixations_list = []
    all_list = []

    for row in reader:
        all_list.append([float(row['CRX']), float(row['CRY'])])
        if row['Result'] == 'Fixation':
            fixations_list.append([float(row['CRX']), float(row['CRY'])])

    file1.close()

    fig = plt.figure()
    myPlot = fig.add_subplot(111)

    myPlot.set_title("Game Gaze ~ test subject 1")
    myPlot.set_xlabel('Pixels')
    myPlot.set_ylabel('Pixels')
    myPlot.axis([0, 1600, 0, 900])

    plt.ion()
    plt.show()

    # for row in all_list:
    myPlot.plot(all_list, color='r', label='Fixations', zorder=1)
    fig.canvas.draw()

    for row in fixations_list:
        myPlot.scatter(row[0], row[1], s=5, color='b', label='Fixations', zorder=10)
        plt.draw()


def testAnimated():
    fig = plt.figure()
    ax1 = fig.add_subplot(1, 1, 1)

    pullData = np.genfromtxt('Data - ResultRaw.csv', delimiter=',', skip_header=10,
                             skip_footer=10, names=['CRX', 'CRY'])


    plt.ion()
    plt.show()

    xar = []
    yar = []
    for eachLine in pullData:
        if len(eachLine) > 1:
            x, y = str(eachLine).split(',')
            xar.append(float(x[1:]))
            yar.append(float(y[:-1]))
            print eachLine
            ax1.clear()
            ax1.autoscale(False, axis='both')
            ax1.axis([0, 1600, 0, 900])
            ax1.plot(xar, yar)
            fig.canvas.draw()




# testPlotSaccadesOnly()
testPlotWithFixations()
#testthree()
#testAnimated()

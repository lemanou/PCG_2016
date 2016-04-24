__author__ = 'Eleftherios'
# -*- coding: utf-8 -*-
"""
Created on Mar 25 2016
PCG Thesis - Correct fixations
Main program.
---------------------------------------------------------
"""
import csv

fixationradius = ''
exportfilename = "Data - ResultRaw" + str(fixationradius) + ".csv"
inputfiles = {"Gazes For LoadSavedLevel 1.csv", "Gazes For scene 2.csv"}
path = "2016.04.21/Panos/"

def max_xvalue(inputlist):
    return max([sublist[0] for sublist in inputlist])


def min_xvalue(inputlist):
    return min([sublist[0] for sublist in inputlist])


def max_yvalue(inputlist):
    return max([sublist[-1] for sublist in inputlist])


def min_yvalue(inputlist):
    return min([sublist[-1] for sublist in inputlist])


def CheckRange(csx, csy, comparing_list):
    maxx = 0.0
    minx = 0.0
    maxy = 0.0
    miny = 0.0

    if len(comparing_list) > 0:
        # print "maxx: ", max_xvalue(comparing_list), "minx: ", min_xvalue(comparing_list), "maxY: ", max_yvalue(comparing_list), " minY: ", min_yvalue(comparing_list)
        maxx = max_xvalue(comparing_list)
        minx = min_xvalue(comparing_list)
        maxy = max_yvalue(comparing_list)
        miny = min_yvalue(comparing_list)

    if (abs(float(maxx) - float(csx)) <= fixationradius and abs(float(minx) - float(csx)) <= fixationradius and abs(
                float(maxy) - float(csy)) <= fixationradius and abs(float(miny) - float(csy)) <= fixationradius):
        return True
    return False


def main(inputfile):
    reader = csv.DictReader(inputfile)

    new_rows_list = []
    comparing_list = []

    header = ['CRX', 'CRY', 'IsFixated', 'State', 'TimeStamp', ' ', 'Result']
    # new_rows_list.append(new_row)
    for row in reversed(list(reader)):
        csx = row['CRX'][1:]
        csy = row['CRY'][:-1]
        if float(csx) == 0 or float(csy) == 0:
            continue  # skip this line
        # print float(csx) + float(csy)
        if row['IsFixated'] == 'TRUE':
            new_row = [csx, csy, row['IsFixated'], row['State'], row['TimeStamp'], row['Result'], 'Fixation']
            new_rows_list.append(new_row)
            comparing_list.append([csx, csy])
        else:
            if CheckRange(csx, csy, comparing_list):
                new_row = [csx, csy, row['IsFixated'], row['State'], row['TimeStamp'], row['Result'], 'Fixation']
                new_rows_list.append(new_row)
                comparing_list.append([csx, csy])
            else:
                new_row = [csx, csy, row['IsFixated'], row['State'], row['TimeStamp'], row['Result'], 'Saccade']
                new_rows_list.append(new_row)
                comparing_list = []

    new_rows_list.append(header)
    new_rows_list.reverse()
    inputfile.close()

    # Do the writing
    file2 = open(exportfilename, 'wb')
    writer = csv.writer(file2)
    writer.writerows(new_rows_list)
    file2.close()


for tmpfile in inputfiles:
    main(tmpfile)

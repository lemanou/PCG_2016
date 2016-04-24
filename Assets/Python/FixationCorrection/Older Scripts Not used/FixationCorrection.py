# -*- coding: utf-8 -*-
"""
Created on Mar 24 2016
PCG Thesis - Correct fixations
Main program.
---------------------------------------------------------
"""
import csv
import itertools


def CheckPreviousPositions(myList):
    """

    :rtype : list
    """
    for i, row in reversed(list(enumerate(myList))):
        print row

    return myList


def main():
    file1 = open("Data - Copy.csv", 'rb')
    reader1, reader2 = itertools.tee(csv.DictReader(file1))  # this creates two copies of file iterators for the file
    next(reader2)  # skip first line in second filehandle

    new_rows_list = []
    new_row = ['CSX', 'CSY', 'IsFixated', 'State', 'TimeStamp', ' ', 'Result']
    new_rows_list.append(new_row)

    for row, next_row in itertools.izip(reader1, reader2):
        # print nextRow['IsFixated']
        if row['IsFixated'] == 'TRUE':
            new_row = [row['CSX'], row['CSY'], row['IsFixated'], row['State'], row['TimeStamp'], row['Result'],
                       'Fixation']
            new_rows_list.append(new_row)
        else:
            new_row = [row['CSX'], row['CSY'], row['IsFixated'], row['State'], row['TimeStamp'], row['Result'],
                       'Saccade']
            new_rows_list.append(new_row)
            if next_row['IsFixated'] == 'TRUE':
                new_rows_list = CheckPreviousPositions(new_rows_list)
                return
    file1.close()

    # Do the writing
    file2 = open("Data - Result.csv", 'wb')
    writer = csv.writer(file2)
    writer.writerows(new_rows_list)
    file2.close()


main()

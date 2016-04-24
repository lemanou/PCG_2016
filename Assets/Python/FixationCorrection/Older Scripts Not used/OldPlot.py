# -*- coding: utf-8 -*-
"""
Created on Sun Nov 10 10:31:23 2014

Information.dk mining

plottinByTag
=========================================================
Plotting calculated sentiments by tag.
---------------------------------------------------------
"""

import matplotlib.pyplot as plt
from collections import Counter, defaultdict


def createList(tmpList):
    '''
    Create a list from 1 to n for the bins (n=number of articles with tag)
    '''
    resultList = []
    for i, val in enumerate(tmpList):
        # To start from article count representation from 1
        resultList.append(i+1)
    return resultList


def tagSentPlottingD(mydefaultDict):
    '''
    Plot the articles from the dictionary
    '''
    # Loop through the tags
    for tag in mydefaultDict:
        # Loop through the lists
        for i, sentList in enumerate(mydefaultDict[tag]):
            bodyAvgList = []
            commAvgList = []
            # Loop through the lists in the lists
            for j, myList in enumerate(sentList):
                # Seperate the contents from body and comments average values
                floatBAvg = float(myList['body']['Average'])
                bodyAvgList.append(floatBAvg)
                floatCAvg = float(myList['comments']['Average'])
                commAvgList.append(floatCAvg)

            # Create a list from 1 to n for the bins
            # (n=number of articles with tag)
            nOfArt = createList(bodyAvgList)

            # Plotting
            plt.figure(tag)
            width = 0.3
            plt.bar(nOfArt, bodyAvgList, width=width,
                    color='blue', alpha=0.5, label='Body')
            plt.bar(nOfArt, commAvgList, width=width,
                    color='red', alpha=0.5, label='Comments')
            plt.legend(loc='upper right')
            plt.xlabel("Articles")
            plt.ylabel("Sentiment Average")
            plt.title("Tag: " + tag)
            plt.savefig(tag + ".png")

    plt.show()


def startPlotting(dbConn):
    '''
    Function used to fetch and plot specific articles
    '''
    print "===startPlotting: Checking DB==="
    tagList = []
    tmpList = []
    # Fetch all "normal" articles from the DB
    for article in dbConn:
        if (len(article) is 7):
            myArticle = dataToCouchDB.loadArticleFromDB(dbConn, article)
            # Get the tags, split them
            if (myArticle['tags']):
                tmpList = myArticle['tags'].split()
                # Keep words bigger than 3
                for word in tmpList:
                    if len(word) > 3:
                        tagList.append(word)

    # Find the 3 most common tags to plot
    counts = Counter(tagList)
    commonTagList = counts.most_common(3)

    print "===startPlotting: Fetching tags="
    # Create a dictionary of lists using defaultdict
    d = defaultdict(list)
    for tag, number in commonTagList:
        sentList = []
        for article in dbConn:
            if (len(article) is 7):
                myArticle = dataToCouchDB.loadArticleFromDB(dbConn, article)
                # with all the tags
                if (tag in myArticle['tags']):
                    sentList.append(myArticle['sentiment'])

        d[tag].append(sentList)

    print "===startPlotting: Plotting======"
    tagSentPlottingD(d)

    return "===Plotting complete==="


def main():
    myDBname = 'information_dk_articles'
    dbConnection = dataToCouchDB.getConnectionDB(myDBname)
    startPlotting(dbConnection)


if __name__ == "__main__":
    main()

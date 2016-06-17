import csv
import matplotlib.pyplot as plt
from matplotlib.font_manager import FontProperties
import scipy as sp
import numpy as np
from scipy import stats


def create_count_list(a_list):
    count_list = []
    for i in range(1, len(a_list) + 1):
        count_list.append(i)
    return count_list


def aic():
    # Akaike information criterion
    # data = stats.gamma.rvs(2, loc=1.5, scale=2, size=100000)
    fitted_params = sp.stats.gamma.fit(ev_list)
    k = len(fitted_params)
    logLik = np.sum(stats.gamma.logpdf(ev_list, fitted_params[0], loc=fitted_params[1], scale=fitted_params[2]))
    aic = 2 * k - 2 * logLik
    print aic


with open('data.csv') as csv_file:
    reader = csv.DictReader(csv_file)
    user_count = 1
    count = 0
    ev_list = []
    totals = []
    for i in range(0, 15):
        totals.append(0)
    for row in reader:
        tmp_float = float(row['ev'])
        ev_list.append(tmp_float)
        # print(row['ev'], row['user'], count)
        totals[count] += tmp_float
        count += 1
        if count == 15:
            plt.plot(create_count_list(ev_list), ev_list, label=user_count)
            user_count += 1
            count = 0
            # aic() # skipped
            del ev_list[:]

    plt.xlabel('Polynomial degrees')
    plt.ylabel('Explained Variance score')
    font_p = FontProperties()
    font_p.set_size('small')
    plt.legend(loc='center right', bbox_to_anchor=(1.1, 0.5), shadow=True, title="Testers", prop=font_p, ncol=2)
    plt.axis([0, 16, 0, 1])
    plt.savefig('PlottinEV' + str(user_count - 1) + '.png', fmt='png', dpi=100)
    plt.show()

    for i in range(0, len(totals)):
        totals[i] /= 50  # 50 samples in total for each entry
    plt.plot(create_count_list(totals), totals)
    plt.xlabel('Polynomial degrees')
    plt.ylabel('Explained Variance score totals')
    plt.axis([0, 16, 0, 1])
    plt.savefig('Plottin_Totals_' + str(user_count - 1) + '.png', fmt='png', dpi=100)
    plt.show()

import csv
import matplotlib.pyplot as plt
from matplotlib.font_manager import FontProperties


def create_count_list(a_list):
    count_list = []
    for i in range(1, len(a_list) + 1):
        count_list.append(i)
    return count_list


with open('data.csv') as csvfile:
    reader = csv.DictReader(csvfile)
    user_count = 1
    count = 0
    ev_list = []
    for row in reader:
        ev_list.append(row['ev'])
        # print(row['ev'], row['user'], count)
        count += 1
        if count == 15:
            plt.plot(create_count_list(ev_list), ev_list, label=user_count)
            user_count += 1
            count = 0
            del ev_list[:]

    plt.xlabel('Polynomial degrees')
    plt.ylabel('Explained variance score')
    font_p = FontProperties()
    font_p.set_size('small')
    plt.legend(loc='center right', bbox_to_anchor=(1.1, 0.5), shadow=True, title="Testers", prop=font_p, ncol=2)
    plt.savefig('PlottinEV' + str(user_count - 1) + '.png', fmt='png', dpi=100)
    plt.show()

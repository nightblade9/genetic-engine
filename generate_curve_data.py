'''
Given a manually-entered Python mathematical equation, picks random points from x=-1000...1000 and outputs the data for those.
'''

import csv
import random

# User-defined inputs
MIN_X = -500
MAX_X = 1000

# y = x^2 + 2x + 1
EQUATION = lambda x: x**2 + (2*x) + 1
# y=3(x^2 / 8) - 2x/3 + 17
#EQUATION = lambda x: (3 * (x**2) / 8.0) - ((2.0 * x / 3)) + 17

# Trolling
#import math
#EQUATION = lambda x: math.sin(x)

# Internal constants
NUM_POINTS = 5000
OUTPUT_FILE = "data.csv"

x_indicies = []

while len(x_indicies) < NUM_POINTS:
    new_index = random.uniform(MIN_X, MAX_X)
    if not new_index in x_indicies:
        x_indicies.append(new_index)


with open(OUTPUT_FILE, 'wb') as out_file:
    writer = csv.writer(out_file)
    for x in x_indicies:
        writer.writerow([x, EQUATION(x)])

print("Wrote {} data points to {}".format(NUM_POINTS, OUTPUT_FILE))
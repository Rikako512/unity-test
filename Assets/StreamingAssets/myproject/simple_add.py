import csv
import os

def sum_iris_features(data_list):
    results = []
    for row in data_list:
        sum_features = sum(row[:3])  # Sum the first three elements (numeric features)
        species = row[3]  # The fourth element is the species
        results.append((sum_features, species))
    return results
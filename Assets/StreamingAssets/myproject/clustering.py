import pandas as pd
import numpy as np
from scipy.stats import pearsonr
from scipy.cluster.hierarchy import dendrogram, linkage
from scipy.spatial.distance import pdist
from sklearn.preprocessing import StandardScaler

def load_data(data):
    df = pd.DataFrame(data)
    features = df.columns.tolist()
    X = df[features].values
    return X, features

def generate_feature_vector(X, feature1, feature2):
    x = X[:, feature1]
    y_plot = X[:, feature2]
    correlation, _ = pearsonr(x, y_plot)
    thinness = 1 / (np.std(x) * np.std(y_plot))
    return np.array([correlation, thinness])

def cosine_similarity(v1, v2):
    return np.dot(v1, v2) / (np.linalg.norm(v1) * np.linalg.norm(v2))

def analyze_data(data):
    X, features = load_data(data)
    
    feature_vectors = []
    feature_triplets = []
    
    for i in range(len(features)):
        for j in range(i+1, len(features)):
            for k in range(j+1, len(features)):
                feature_vectors.append(generate_feature_vector(X, i, j))
                feature_triplets.append((features[i], features[j], features[k])) 

    feature_vectors = np.array(feature_vectors)
    
    n = len(feature_vectors)
    similarity_matrix = np.zeros((n, n))
    
    for i in range(n):
        for j in range(n):
            similarity_matrix[i, j] = cosine_similarity(feature_vectors[i], feature_vectors[j])
    
    distance_matrix = 1 - similarity_matrix
    condensed_distance_matrix = pdist(distance_matrix)
    
    linkage_matrix = linkage(condensed_distance_matrix, method='centroid')
    
    order = dendrogram(linkage_matrix, no_plot=True)['leaves']
    
    return {
        'order': order,
        'feature_triplets': feature_triplets
    }
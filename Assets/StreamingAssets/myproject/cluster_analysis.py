import numpy as np
import hdbscan
from sklearn.datasets import make_blobs

def perform_cluster_analysis():
    # データ生成
    data, _ = make_blobs(n_samples=1000, centers=4, n_features=2, random_state=0)

    clusterer = hdbscan.HDBSCAN(min_cluster_size=10, gen_min_span_tree=True)
    clusterer.fit(data)

    labels = clusterer.labels_
    cluster_centers = []
    cluster_sizes = []

    for cluster_id in set(labels):
        if cluster_id != -1:
            cluster_points = data[labels == cluster_id]
            center = cluster_points.mean(axis=0)
            cluster_centers.append(center)

            # クラスタのサイズを計算
            cluster_size = len(cluster_points)
            cluster_sizes.append(cluster_size)

    cluster_centers = np.array(cluster_centers)
    cluster_sizes = np.array(cluster_sizes)

    # サイズに応じてマーカーサイズを決定
    marker_sizes = 1000 * (cluster_sizes / cluster_sizes.max())

    return {
        "number_of_clusters": len(set(labels)) - (1 if -1 in labels else 0),  # ノイズを除外
        "cluster_centers": cluster_centers.tolist(),
        "cluster_sizes": cluster_sizes.tolist(),
        "marker_sizes": marker_sizes.tolist()
    }

if __name__ == "__main__":
    result = perform_cluster_analysis()
    print(f"Number of clusters: {result['cluster_count']}")
    print(f"Cluster centers:\n{result['cluster_centers']}")

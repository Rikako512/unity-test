import numpy as np
import hdbscan

def perform_hdbscan_analysis(data):
    # �f�[�^��NumPy�z��ɕϊ�
    data = np.array(data, dtype=float)

    # HDBSCAN�N���X�^�����O
    clusterer = hdbscan.HDBSCAN(min_cluster_size=5, gen_min_span_tree=True)
    clusterer.fit(data)

    labels = clusterer.labels_
    # unique_labels = np.unique(labels)
    
    cluster_centers = []
    cluster_sizes = []

    for cluster_id in set(labels):
        if cluster_id != -1:  # �m�C�Y�|�C���g�����O
            cluster_points = data[labels == cluster_id]
            center = cluster_points.mean(axis=0)
            cluster_centers.append(center)
            cluster_sizes.append(int(len(cluster_points)))
     
    cluster_centers = np.array(cluster_centers)
    cluster_sizes = np.array(cluster_sizes)

    # �T�C�Y�ɉ����ă}�[�J�[�T�C�Y������
    marker_sizes = 500 * (cluster_sizes / cluster_sizes.max()) if len(cluster_sizes) > 0 else []

    # �m�C�Y�|�C���g�̐�
    noise_points = np.sum(labels == -1)

    return {
        "number_of_clusters": len(cluster_centers),
        "cluster_centers": cluster_centers.tolist(),
        "cluster_sizes": cluster_sizes.tolist(),
        "noise_points": int(noise_points),
        "marker_sizes": marker_sizes.tolist()
    }


if __name__ == "__main__":
    result = perform_hdbscan_analysis()
    print(f"Number of clusters: {result['cluster_count']}")
    print(f"Cluster centers:\n{result['cluster_centers']}")
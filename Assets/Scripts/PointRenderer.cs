using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PointRenderer : MonoBehaviour {

    //********Public Variables********
    // Bools for editor options
    public bool renderParticles =  true;
    public bool renderPrefabsWithColor = true;
    public bool only1plot = false; // 散布図行列上で一つの散布図だけ描画する

    // Indices for columns to be assigned
    public int column1 = 0;
    public int column2 = 1;
    public int column3 = 2;

    // Full column names from CSV (as Dictionary Keys)
    public string xColumnName;
    public string yColumnName;
    public string zColumnName;

    // Scale of particlePoints within graph, WARNING: Does not scale with graph frame
    private float plotScale = 10;

    // Scale of the prefab particlePoints
    [Range(0.0f, 0.5f)]
    public float pointScale = 0.25f;

    // Changes size of particles generated
    [Range(0.0f, 2.0f)]
    public float particleScale = 5.0f;

    // The prefab for the data particlePoints that will be instantiated
    public GameObject PointPrefab;

    // Object which will contain instantiated prefabs in hiearchy
    public GameObject PointHolder;

    // Objects which will contain axis labels in hiearchy
    public GameObject XLabels;
    public GameObject YLabels;
    public GameObject ZLabels;

    // Color for the glow around the particlePoints
    private Color GlowColor;

    //********Private Variables********
    // Minimum and maximum values of columns
    private float xMin;
    private float yMin;
    private float zMin;

    private float xMax;
    private float yMax;
    private float zMax;

    // Number of rows
    private int rowCount;

    // Particle system for holding point particles
    private ParticleSystem.Particle[] particlePoints;

    void Awake()
    {

    }

    // Use this for initialization
    void Start ()
	  {
        Debug.Log("---------- PointRenderer開始 ----------");

        // Store dictionary keys (column names in CSV) in a list
        List<string> columnList = new List<string>(CSVData.pointList[1].Keys);

        // Assign column names according to index indicated in columnList
        xColumnName = columnList[column1];
        yColumnName = columnList[column2];
        zColumnName = columnList[column3];

        xMax = Convert.ToSingle(CSVData.min_maxList[xColumnName][1]);
        yMax = Convert.ToSingle(CSVData.min_maxList[yColumnName][1]);
        zMax = Convert.ToSingle(CSVData.min_maxList[zColumnName][1]);

        xMin = Convert.ToSingle(CSVData.min_maxList[xColumnName][0]);
        yMin = Convert.ToSingle(CSVData.min_maxList[yColumnName][0]);
        zMin = Convert.ToSingle(CSVData.min_maxList[zColumnName][0]);

        AssignLabels();
        PlacePrefabPoints();

        if (only1plot == true)
        {
            // column1, column2, column3が特定の場合のみ実行
            if (column1 == 1 && column2 == 0 && column3 == 3)
            {
                PlacePrefabPoints();
            }
        }   

        // If statement to turn particles on and off
        if ( renderParticles == true)
        {
            // Call CreateParticles() for particle system
            CreateParticles();

            // Set particle system, for point glow- depends on CreateParticles()
            GetComponent<ParticleSystem>().SetParticles(particlePoints, particlePoints.Length);
        }

        Debug.Log("---------- PointRenderer終了：(" + column1 + ", " + column2 + ", " + column3 + ") ----------");
    }


    // Update is called once per frame
    void Update ()
    {

    }

    // Places the prefabs according to values read in
    private void PlacePrefabPoints()
    {
      Debug.Log("---------- PlacePrefabPoints 開始 ----------");
      // Get count (number of rows in table)
      rowCount = CSVData.pointList.Count;

      for (var i = 0; i < rowCount; i++)
      {

        // Set x/y/z, standardized to between 0-1
        float x = (Convert.ToSingle(CSVData.pointList[i][xColumnName]) - xMin) / (xMax - xMin);
        float y = (Convert.ToSingle(CSVData.pointList[i][yColumnName]) - yMin) / (yMax - yMin);
        float z = (Convert.ToSingle(CSVData.pointList[i][zColumnName]) - zMin) / (zMax - zMin);

        // Create vector 3 for positioning particlePoints
        Vector3 position = new Vector3 (x, y, z) * plotScale;

        //instantiate as gameobject variable so that it can be manipulated within loop
        GameObject dataPoint = Instantiate (PointPrefab, Vector3.zero, Quaternion.identity);

        // Make child of PointHolder object, to keep particlePoints within container in hiearchy
        dataPoint.transform.parent = PointHolder.transform;

        // Position point at relative to parent
        dataPoint.transform.localPosition = position;
        pointScale = 0.25f;
        dataPoint.transform.localScale = new Vector3(pointScale, pointScale, pointScale);

        // Converts index to string to name the point the index number
        string dataPointName = i.ToString();

        // Assigns name to the prefab
        dataPoint.transform.name = dataPointName;

        // データ点の色を指定
        if (renderPrefabsWithColor == true)
        {
          // Sets color according to x/y/z value
          dataPoint.GetComponent<Renderer>().material.color = new Color(x, y, z, 1.0f);

          // Activate emission color keyword so we can modify emission color
          dataPoint.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");

          dataPoint.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(x, y, z, 1.0f));

        }

      }

      Debug.Log("---------- PlacePrefabPoints 終了 ----------");
    }

    private void CreateParticles()
    {
      rowCount = CSVData.pointList.Count;

      particlePoints = new ParticleSystem.Particle[rowCount];

      for (int i = 0; i < CSVData.pointList.Count; i++)
      {
        // Convert object from list into float
        float x = (Convert.ToSingle(CSVData.pointList[i][xColumnName]) - xMin) / (xMax - xMin);
        float y = (Convert.ToSingle(CSVData.pointList[i][yColumnName]) - yMin) / (yMax - yMin);
        float z = (Convert.ToSingle(CSVData.pointList[i][zColumnName]) - zMin) / (zMax - zMin);

        // Set point location
        particlePoints[i].position = new Vector3(x, y, z) * plotScale;

        // Set point color
        particlePoints[i].startColor = new Color(x, y, z, 1.0f);
        particlePoints[i].startSize = particleScale;
      }

    }

    private void AssignLabels()
    {
        XLabels.transform.Find("X_Title").gameObject.GetComponent<TextMesh>().text = xColumnName;
        YLabels.transform.Find("Y_Title").gameObject.GetComponent<TextMesh>().text = yColumnName;
        ZLabels.transform.Find("Z_Title").gameObject.GetComponent<TextMesh>().text = zColumnName;

        XLabels.transform.Find("X_Min_Lab").gameObject.GetComponent<TextMesh>().text = xMin.ToString("0.0");
        XLabels.transform.Find("X_Mid_Lab").gameObject.GetComponent<TextMesh>().text = (xMin + (xMax - xMin) / 2f).ToString("0.0");
        XLabels.transform.Find("X_Max_Lab").gameObject.GetComponent<TextMesh>().text = xMax.ToString("0.0");

        YLabels.transform.Find("Y_Min_Lab").gameObject.GetComponent<TextMesh>().text = yMin.ToString("0.0");
        YLabels.transform.Find("Y_Mid_Lab").gameObject.GetComponent<TextMesh>().text = (yMin + (yMax - yMin) / 2f).ToString("0.0");
        YLabels.transform.Find("Y_Max_Lab").gameObject.GetComponent<TextMesh>().text = yMax.ToString("0.0");

        ZLabels.transform.Find("Z_Min_Lab").gameObject.GetComponent<TextMesh>().text = zMin.ToString("0.0");
        ZLabels.transform.Find("Z_Mid_Lab").gameObject.GetComponent<TextMesh>().text = (zMin + (zMax - zMin) / 2f).ToString("0.0");
        ZLabels.transform.Find("Z_Max_Lab").gameObject.GetComponent<TextMesh>().text = zMax.ToString("0.0");

    }
}

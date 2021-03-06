﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class GameHelper : MonoBehaviour
{
    const string ConsumerKey = "UtRpyiTLdvEEWKmCxpNTGX5IB1kUe644";
    const int WaitTime = 10;

    /// <summary>
    /// Url propertyes
    /// </summary>
    string Url = "";
    public Transform myMap;
    int _multiplier = 2;

    public Renderer maprender;
    public Text StatusText;

    public bool gpsFix { get; set; }

    /// <summary>
    /// Latitude, Longitude
    /// </summary>
    private Vector2 PlayerPosition =
       new Vector2(50.254620f, 28.658708f);

    private double tempLat;
    private double tempLon;

    Vector3 _iniRef;
    public Vector3 IniRef
    {
        get { return _iniRef; }
        set { _iniRef = value; }
    }

    int _zoom = 17;
    string _maptype = "map";

    private LocationInfo _loc;
    float _download = 0;
    public string Status { set { StatusText.text = value; } }

    int _counter;


    IEnumerator Start()
    {

        Input.location.Start(5, 5);
        Input.compass.enabled = true;
        Status = "Initializing Location Services..";

        // Wait until service initializes
        while (Input.location.status ==
            LocationServiceStatus.Initializing &&
            _counter < WaitTime)
        {
            yield return new WaitForSeconds(1);
            Status = "Wait " + _counter;
            _counter++;
        }

        if (_counter >= WaitTime)
        {
            Status = "_counter >= WaitTime";
            yield return new WaitForSeconds(4);
            Application.Quit();
            yield return null;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Status = "Input.location.status == LocationServiceStatus.Failed";
            yield return new WaitForSeconds(4);
            Application.Quit();
            yield return null;
        }
        else
        {
            LocationInfo loc = Input.location.lastData;
            yield return new WaitForSeconds(2);
            /// Только для телефонов
            // PlayerPosition.x = loc.latitude;
            // PlayerPosition.y = loc.longitude;

            //Set Position
            _iniRef.x = (float)((PlayerPosition.y * 20037508.34 / 180) / 100);
            _iniRef.z = (float)(System.Math.Log(System.Math.Tan((90 + PlayerPosition.x)
                * System.Math.PI / 360)) / (System.Math.PI / 180));
            _iniRef.z = (float)((_iniRef.z * 20037508.34 / 180) / 100);
            _iniRef.y = 0;

            LoadMap(PlayerPosition);
        }
    }


    private void LoadMap(Vector2 playerPosition)
    {
        Url = "http://open.mapquestapi.com/staticmap/v4/getmap?key=" +
            ConsumerKey + "&size=1280,1280&zoom=" +
            _zoom + "&type=" + _maptype +
            "&center=" + PlayerPosition.x + "," + PlayerPosition.y;

        StartCoroutine(LoadMap());
    }

    private IEnumerator LoadMap()
    {
        WWW www = new WWW(Url);

        while (!www.isDone)
        {
            _download = (www.progress);
            Status = "Updating map " + System.Math.Round(_download * 100) + " %";
            yield return null;
        }

        if (www.error == null)
        {
            Status = "Updating map 100 %\nMap Ready!";
            yield return new WaitForSeconds(0.5f);
            maprender.material.mainTexture = null;
            Texture2D tmp;
            tmp = new Texture2D(1280, 1280, TextureFormat.RGB24, false);
            maprender.material.mainTexture = tmp;
            www.LoadImageIntoTexture(tmp);
        }
        else
        {
            print("Map Error:" + www.error);
            Status = "Map Error:" + www.error;
            yield return new WaitForSeconds(1);
            maprender.material.mainTexture = null;
        }

        tempLat = PlayerPosition.x;
        tempLon = PlayerPosition.y;

        maprender.enabled = true;
        ReSet();
        ReScale();
    }

    void ReSet()
    {
        Vector3 newPosition = new Vector3();
        newPosition.x = (float)(((tempLon * 20037508.34 / 180) / 100) - _iniRef.x);
        newPosition.z = (float)(System.Math.Log(System.Math.Tan((90 + tempLat)
            * System.Math.PI / 360)) / (System.Math.PI / 180));
        newPosition.z = (float)(((newPosition.z * 20037508.34 / 180) / 100) - _iniRef.z);
        transform.position = newPosition;
    }

    void ReScale()
    {
        Vector3 newScale = myMap.localScale;
        newScale.x = (float)(_multiplier * 100532.244f / (Mathf.Pow(2, _zoom)));
        newScale.z = newScale.x;
        myMap.localScale = newScale;
    }
}

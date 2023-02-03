using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;

using UnityEngine.UI;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using TMPro;
public class socketLive : MonoBehaviour
{
   Thread mThread;
      //public string connectionIP = "127.0.0.1";
      public int connectionPort = 8050;
      IPAddress localAdd;
      
      TcpListener listener;
      TcpClient client;
      bool running;
    public List<List<double>> myColorValues;
    //public bool startListen;
    Color[] colors;
    Root stuff;

    private string baseName;
    Mesh mesh;
    TextMeshPro textmeshPro;
    Vector3[] vertices;
    TextAsset asset;
    private int idx;
    [System.Serializable]
   public class Root
    {
        public int win_idx;
        public List<List<double>> mylist { get; set; }
    }
    // public void just_play()
    // {
        
    //     startListen= !startListen;
    //     Debug.Log(running);
        
    // }
    void Start()
    {
        textmeshPro = GameObject.FindWithTag("s_start").GetComponent<TextMeshPro>();
        this.idx = 0;
        textmeshPro.SetText("Window Number: " + this.idx);
        //startListen = false;
        Application.targetFrameRate = 60;
        this.mesh = this.GetComponent<MeshFilter>().mesh;
        this.vertices = mesh.vertices;

        this.colors = new Color[vertices.Length];
        // print(mesh.vertices.Length);
        mesh.RecalculateNormals();
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
        
       
    }
    void Update()
    {
        this.mesh.colors = colors;
        this.textmeshPro.SetText("Window Number: " + this.idx);
  
        
    }
    public static string GetLocalIPAddress()
      {
          var host = Dns.GetHostEntry(Dns.GetHostName());
          print(host);
          foreach (var ip in host.AddressList)
          {
              if (ip.AddressFamily == AddressFamily.InterNetwork)
              {
                  return ip.ToString();
              }
          }
          throw new System.Exception("No network adapters with an IPv4 address in the system!");
      }
    void GetInfo()
      {
        
        //   localAdd = IPAddress.Parse(connectionIP);
          listener = new TcpListener(IPAddress.Any, connectionPort);
          listener.Start();
        
          client = listener.AcceptTcpClient();
          running = true;


          
          while (running)
          {
              try {
                    listener.Start();
                    
                    client = listener.AcceptTcpClient();
                  Connection();
              } catch(Exception e) {
                running = false;
                
                print(e);
                
                // if (stop_idx >60*5){

                //     client.Close();
                //     listener.Stop();
                //     running = false;
                    
                // }
                //   //listener.AcceptTcpClient();
                //   stop_idx++;
                  

                //   client.Close();
                // listener.Stop();
              }
                  
              
              
          }
          running = false;
          client.Close();
          listener.Stop();
          //mThread.Abort();
          print("closing");
      }
      void Connection()
      {
          NetworkStream nwStream = client.GetStream();
          byte[] buffer = new byte[70940];
          
        // TODO: MUST CHANGE
          int bytesRead = nwStream.Read(buffer, 0, 70940);
          // Passing data as strings, not ideal but easy to use
          string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

          if (dataReceived != null)
          {
              if (dataReceived == "stop")
              {
              // Can send a string "stop" to kill the connection
                  running = false;
              }
              else
              {
                  
                  stuff = JsonConvert.DeserializeObject<Root>(dataReceived);
                  for (int i = 0; i < vertices.Length; i++) {
                        
                        colors[i] = new Color((float)stuff.mylist[i][0]/255,(float)stuff.mylist[i][1]/255,(float)stuff.mylist[i][2]/255,(float)stuff.mylist[i][3]/255);
                    }
                    //print(stuff.win_idx);
                  this.idx = stuff.win_idx;
                  
              }
          } 
      }



    
}


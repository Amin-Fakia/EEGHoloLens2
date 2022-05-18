using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;

using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
public class WS_Client : MonoBehaviour
{
   Thread mThread;
      public string connectionIP = "127.0.0.1";
      public int connectionPort = 8500;
      IPAddress localAdd;
      TcpListener listener;
      TcpClient client;
      bool running;
        public List<List<double>> myColorValues;
        Vector3 position = Vector3.zero; 
        Color[] colors;
    Root stuff;
    private string baseName;
    Mesh mesh;
    Vector3[] vertices;
    TextAsset asset;
    [System.Serializable]
   public class Root
    {
        public List<List<double>> mylist { get; set; }
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        this.mesh = this.GetComponent<MeshFilter>().mesh;
        this.vertices = mesh.vertices;
        this.colors = new Color[vertices.Length];
        
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
       
    }
    void Update()
    {
        this.mesh.colors = colors;
        
    }
    public static string GetLocalIPAddress()
      {
          var host = Dns.GetHostEntry(Dns.GetHostName());
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
          localAdd = IPAddress.Parse(connectionIP);
          listener = new TcpListener(IPAddress.Any, connectionPort);
          listener.Start();

          client = listener.AcceptTcpClient();
    

          running = true;
          while (running)
          {
              Connection();
          }
          listener.Stop();
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
                        //colors[i] = Color.red;
                        colors[i] = new Color((float)stuff.mylist[i][0]/255,(float)stuff.mylist[i][1]/255,(float)stuff.mylist[i][2]/255,(float)stuff.mylist[i][3]/255);
                    }
                  
                    //print("yo");
                  // Convert the received string of data to the format we are using
                  //position = 10f * StringToVector3(dataReceived);
                  //print("moved");
                  
                  //nwStream.Write(buffer, 0, bytesRead);
              }
          } else {
              print("yo wtf");
          }
      }

      // Use-case specific function, need to re-write this to interpret whatever data is being sent
    //   public static Vector3 StringToVector3(string sVector)
    //   {
    //       // Remove the parentheses
    //       if (sVector.StartsWith("(") && sVector.EndsWith(")"))
    //       {
    //           sVector = sVector.Substring(1, sVector.Length - 2);
    //       }

    //       // Split the elements into an array
    //       string[] sArray = sVector.Split(',');

    //       // Store as a Vector3
    //       Vector3 result = new Vector3(
    //           float.Parse(sArray[0]),
    //           float.Parse(sArray[1]),
    //           float.Parse(sArray[2]));

    //       return result;
    //   }


    
}

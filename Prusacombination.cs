using Godot;
using System;

namespace OctoprintClient
{
    public class Prusacombination : Spatial
    {
        //private GD.OctoprintConnection octoprint;
        // Member variables here, example:
        // private int a = 2;
        // private string b = "textvar";
        private Spatial xAxis = new Spatial();
        Spatial yAxis = new Spatial();
        Spatial zAxis = new Spatial();
        OctoprintConnection octoprint = new OctoprintConnection();

        public Spatial XAxis { get => xAxis; set => xAxis = value; }

        public override void _Ready()
        {
            xAxis = GetNode("Mk3Z/Mk3X") as Spatial;
            yAxis = GetNode("Mk3Y") as Spatial;
            zAxis = GetNode("Mk3Z") as Spatial;
            // Called every time the node is added to the scene.
            // Initialization here
            octoprint.endPoint = "http://192.168.11.54/";
            octoprint.apiKey = "7C2A3AAA91BD42B6BC693B26C0DBBF25";
            //print("Rest Client looking");
            GD.Print("Rest Client looking...");
            string strResponse = octoprint.makeRequest("/api/files");
            GD.Print(strResponse);
        }

        public override void _Process(float delta)
        {   
			string strResponse = octoprint.makeRequest("/api/job");
            GD.Print(strResponse);
            XAxis.Translation= new Vector3(50,0,0);
            yAxis.Translation= new Vector3(0,0,0);
            zAxis.Translation= new Vector3(-50,1,0);
            //Console.Write(strResponse);
            // Called every frame. Delta is time since last frame.
            // Update game logic here.

        }
    }
}
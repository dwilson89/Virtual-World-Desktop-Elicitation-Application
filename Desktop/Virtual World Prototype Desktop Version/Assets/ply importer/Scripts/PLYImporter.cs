using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class PLYImporter : MonoBehaviour 
{
	private List<Vector3> points;
	private List<Color> pointColors;
	private List<int> faces;
	public string meshLocation;

	private static Vector3 centerPoint;

	void Start () 
	{
		ReadBinaryFile(meshLocation);
	}
	
	public void ReadBinaryFile(string file) 
	{
		// Worker Variables
		int count = 0;
		points = new List<Vector3>();
		pointColors = new List<Color>();
		faces = new List<int>();
		int numvertices = 0;
		int numfaces = 0;
		
		//---------------------------------------------------------------------
		// Read in the header information.
		//---------------------------------------------------------------------
		try {
			Stream s = File.Open (file, FileMode.Open);
			StreamReader ar = new StreamReader(s);
			string line = "";
			while (line != "end_header") {
				line = ar.ReadLine ();
				string[] parts = line.Split (' ');
				if (parts.Length == 3 && parts[1] == "vertex") {
					numvertices = int.Parse (parts[2]);
				} else if (parts.Length == 3 && parts[1] == "face") {
					numfaces = int.Parse (parts[2]);	
				}
			}
			s.Close ();
		} catch {
			return;	
		}
		
		//---------------------------------------------------------------------
		// Read past all of the ASCII header data.
		//---------------------------------------------------------------------
		Stream test = File.Open(file, FileMode.Open);
		BinaryReader tr = new BinaryReader(test);
		while (true) {
			char c = tr.ReadChar ();
			if (c == '\n') { 
				count++; 
				if (count == 14) break;	
			}
		}
		
		//---------------------------------------------------------------------
		// Read in the binary file for vertex and face data.
		//---------------------------------------------------------------------
		for (int i = 0; i < numvertices; i++) 
		{

			// First 3 bytes represent XYZ vertex position.
			float x = tr.ReadSingle ();
			float y = tr.ReadSingle ();
			float z = tr.ReadSingle ();
			Vector3 point = new Vector3(x, y, z);
			centerPoint += point;
			points.Add (point);

			// Next 4 bytes represent RGBA vertex color.
			float r = tr.ReadByte () / 512.0f;
			float g = tr.ReadByte () / 512.0f;
			float b = tr.ReadByte () / 512.0f;
			float a = tr.ReadByte () / 512.0f;
			pointColors.Add (new Color(r, g, b, a));
		}

		centerPoint /= numvertices;
		transform.position = -centerPoint;
		Camera.main.GetComponent<SmoothMouseOrbit>().UpdateDistance(Mathf.Abs(transform.position.z));

		// Read in each triplet of vertices.
		for (int i = 0; i < numfaces; i++) 
		{
			tr.ReadByte ();
			faces.Add(tr.ReadInt32 ());
			faces.Add(tr.ReadInt32 ());
			faces.Add(tr.ReadInt32 ());
		}
		tr.Close ();
		
		//---------------------------------------------------------------------
		// Create the mesh using vertex and face data.
		//---------------------------------------------------------------------
		Mesh mesh = new Mesh();
		mesh.Clear ();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = points.ToArray();
		mesh.triangles = faces.ToArray();
		mesh.colors = pointColors.ToArray();
		points = null;
		pointColors = null;
		faces = null;
	}
}

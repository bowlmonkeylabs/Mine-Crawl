/// ----------------------------------------------------------------------------------
/// Script to Automatically place offmeshlinks on navmeshes.
/// Originally made by eDmitriy (https://forum.unity.com/members/edmitriy.387921/)
/// Modified by christougher (https://forum.unity.com/members/christougher.787388/ https://forum.arongranberg.com/u/Christougher)
/// Modified and ported to A* PathFinding Project (https://arongranberg.com/astar/)
/// by ToastyStoemp (https://forum.arongranberg.com/u/ToastyStoemp)
///
/// Notes:
/// Currently only works with NodeLink2, though easy to change by replacing all instanced of NodeLink2 with the desired one.
/// For Grid Graphs you might have to enable invertFacingNormal also might be required when using multiple graph types together.
/// 
/// Last edited on 9th February 2021 
/// ----------------------------------------------------------------------------------


using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Sirenix.Utilities;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BML.Scripts.Pathfinding
{

    public class NavMeshLinks_AutoPlacer : GraphModifier
    {
        #region Variables
        
        public bool generateOnPostScan = true;

        public Transform linkPrefab;
        public Transform oneWayLinkPrefab;
        
        [Tooltip("Name of graph to be used to analyzing edges and deciding where to place node links")]
        public string referenceGraphName;
        
        [Tooltip("List of graph names that will have copy of each node link created on them. Basically, list the" +
                 "names of all graphs that will use the node links.")]
        public List<string> graphsToLink = new List<string>();
        public float tileWidth = 5f;

        [Header("OffMeshLinks")] public float maxJumpDownHeight = 5f;
        public float maxJumpUpHeight = 3f;

        public float maxJumpDist = 5f;
        public LayerMask raycastLayerMask = -1;
        public float sphereCastRadius = 1f;
        
        //Max distance between raycast hit and nearest node retrieved. To prevent making links to nodes not even close
        public float maxClosestNodeDist = 3f;

        //how far over to move spherecast away from navmesh edge to prevent detecting the same edge
        public float cheatOffset = .25f;

        //how high up to bump raycasts to check for walls (to prevent forming links through walls)
        public float wallCheckYOffset = 0.5f;
        
        public bool ignoreSameGraph = false;
        public bool onlySameGraph = false;

        [Header("EdgeNormal")] public bool invertFacingNormal = false;
        public bool dontAllignYAxis = true;


        //private List< Vector3 > spawnedLinksPositionsList = new List< Vector3 >();
        private Mesh currMesh;
        private List<Edge> edges = new List<Edge>();

        public float agentRadius = 0.5f;

        public bool enableDebug = false;
        public bool showedFailedHits = false;
        public float normalLength = .5f;
        public float placePosRadius = .2f;
        public float debugLineDuration = 10f;

        private Vector3 ReUsableV3;
        private Vector3 offSetPosY;
        
        [NonSerialized]
        private List<NodeLink2> nodeLinks = new List<NodeLink2>();

        private List<Vector3> placePosList = new List<Vector3>();

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (!enableDebug) return;
            if (edges.IsNullOrEmpty()) return;

            foreach (var edge in edges)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(edge.start, edge.end);
                Gizmos.color = Color.yellow;
                Vector3 normalPos = Vector3.Lerp(edge.start, edge.end, .5f);
                Vector3 normal = Vector3.Cross(edge.end - edge.start, Vector3.up).normalized; //How its cal in this code
                Gizmos.DrawLine(normalPos, normalPos + normal * normalLength);
            }

            if (placePosList.IsNullOrEmpty()) return;

            foreach (var placePos in placePosList)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(placePos, placePosRadius);
            }
        }

        #region GridGen

        public void Generate()
        {
            if (linkPrefab == null) return;
            //agentRadius = NavMesh.GetSettingsByIndex(0).agentRadius;

            edges.Clear();
            //spawnedLinksPositionsList.Clear();

            Debug.Log("Proceeding with generate");
            foreach (NavGraph graph in AstarData.active.graphs)
            {
                Debug.Log($"Considering graph: {graph.name}");
                
                if (graph.name != referenceGraphName)
                    continue;

                if (graph != null && !(graph is PointGraph))
                {
                    Debug.Log($"Starting calculate Edges: {graph.name}");
                    CalcEdges(graph);
                    Debug.Log($"Calculated Edges: {graph.name}");
                }
            }

            Debug.Log($"Starting place tiles");
            PlaceTiles();


#if UNITY_EDITOR
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        }

        public override void OnPostScan()
        {
            if (generateOnPostScan)
                Generate();

            if (nodeLinks == null)
                return;
            
            // Commented this out because it was causing infinite loading times...doesn't seem like its needed
            /*
            foreach (NodeLink2 nodeLink in nodeLinks)
            {
                nodeLink.OnPostScan();
            }
            */
        }

        public override void OnGraphsPostUpdate()
        {
            if (nodeLinks == null)
                return;
            
            foreach (NodeLink2 nodeLink in nodeLinks)
            {
                nodeLink.OnGraphsPostUpdate();
            }
        }

        public void ClearLinks()
        {
            List<NodeLink2> navMeshLinkList = GetComponentsInChildren<NodeLink2>().ToList();
            while (navMeshLinkList.Count > 0)
            {
                GameObject obj = navMeshLinkList[0].gameObject;
                if (obj != null) DestroyImmediate(obj);
                navMeshLinkList.RemoveAt(0);
            }
            
            nodeLinks.Clear();
        }

        private void PlaceTiles()
        {
            if (edges.Count == 0) return;

            ClearLinks();

            placePosList.Clear();
            foreach (Edge edge in edges)
            {
                int tilesCountWidth = (int) Mathf.Clamp(edge.length / tileWidth, 0, 10000);
                float heightShift = 0;


                for (int columnN = 0; columnN < tilesCountWidth; columnN++) //every edge length segment
                {
                    Vector3 placePos = Vector3.Lerp(
                                           edge.start,
                                           edge.end,
                                           (float) columnN / (float) tilesCountWidth //position on edge
                                           + 0.5f / (float) tilesCountWidth //shift for half tile width
                                       ) + edge.facingNormal * Vector3.up * heightShift;

                    placePosList.Add(placePos);
                    //spawn up/down links
                    CheckPlacePos(placePos, edge.facingNormal);
                    //spawn horizontal links
                    CheckPlacePosHorizontal(placePos, edge.facingNormal);
                }
            }
        }




        bool CheckPlacePos(Vector3 pos, Quaternion normal)
        {
            bool result = false;

            Vector3 startPosHorizontal = pos + Vector3.up * wallCheckYOffset - normal * Vector3.forward * agentRadius;
            Vector3 startPos = pos + normal * Vector3.forward * agentRadius * 2 + Vector3.up * wallCheckYOffset;
            Vector3 endPos = startPos - Vector3.up * maxJumpDownHeight * 1.1f;

            //Debug Wall Hit
            if (enableDebug)
            {
                bool hit = Physics.Linecast(startPosHorizontal, startPos, out _, raycastLayerMask.value,
                    QueryTriggerInteraction.Ignore);

                //Only show if success(miss in this case) or config to show anyway
                if (showedFailedHits || !hit)
                {
                    Debug.DrawLine ( startPosHorizontal, startPos, hit ? Color.red : Color.green, debugLineDuration);
                }
            }

            NNConstraint constraint = NNConstraint.Default;
            NNInfo nodeStartInfo = AstarPath.active.GetNearest(pos, constraint);
            NavGraph startGraph = nodeStartInfo.node.Graph;

            RaycastHit raycastHit = new RaycastHit();
            if (!Physics.Linecast(startPosHorizontal, startPos, out _, raycastLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                //Debug Vertical Hit
                if (enableDebug)
                {
                    bool hit = Physics.Linecast(startPos, endPos, out raycastHit, raycastLayerMask.value,
                        QueryTriggerInteraction.Ignore);

                    //Only show if success(hit in this case) or config to show anyway
                    if (showedFailedHits || hit)
                    {
                        Debug.DrawLine ( startPos, endPos, hit ? Color.green : Color.red, debugLineDuration);
                    }
                }
                
                if (Physics.Linecast(startPos, endPos, out raycastHit, raycastLayerMask.value, QueryTriggerInteraction.Ignore))
                {
                    if (enableDebug) Debug.DrawLine(raycastHit.point, raycastHit.point + Vector3.right, Color.cyan, debugLineDuration);
                    NNInfo nodeInfo = AstarPath.active.GetNearest(raycastHit.point, constraint);
                    NavGraph targetGraph = nodeInfo.node.Graph;
                    Vector3 closestPos = nodeInfo.position;

                    if (nodeInfo.node != null && Vector3.Distance(raycastHit.point, closestPos) < maxClosestNodeDist)
                    {
                        if (ignoreSameGraph && targetGraph == startGraph)
                            return false;

                        if (onlySameGraph && targetGraph != startGraph)
                            return false;

                        if (enableDebug)
                        {
                            Debug.DrawLine( pos, closestPos, Color.black, debugLineDuration);
                        }
                        

                        if (Vector3.Distance(pos, closestPos) > 1.1f)
                        {
                            //added these 2 line to check to make sure there aren't flat horizontal links going through walls
                            Vector3 calcV3 = (pos - normal * Vector3.forward * 0.02f);
                            if ((calcV3.y - closestPos.y) > 1f)
                            {

                                //SPAWN NAVMESH LINK
                                Transform typeOfLinkToSpawn = linkPrefab;

                                if (calcV3.y - closestPos.y > maxJumpUpHeight)
                                {
                                    typeOfLinkToSpawn = oneWayLinkPrefab;
                                }
                                
                                //Spawn a copy of the link for each graph listed
                                foreach (var graphName in graphsToLink)
                                {
                                    GraphMask mask = GraphMask.FromGraphName(graphName);
                                    Transform spawnedTransf = Instantiate(typeOfLinkToSpawn, calcV3, normal);

                                    NodeLink2 nodeLink = spawnedTransf.GetComponent<NodeLink2>();
                                    GameObject endPoint = new GameObject("end");
                                    endPoint.transform.position = closestPos;
                                    endPoint.transform.SetParent(spawnedTransf, true);
                                    nodeLink.end = endPoint.transform;
                                    nodeLink.graphMask = mask;

                                    spawnedTransf.SetParent(transform);

                                    nodeLinks.Add(nodeLink);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        bool CheckPlacePosHorizontal(Vector3 pos, Quaternion normal)
        {
            bool result = false;

            Vector3 startPos = pos + normal * Vector3.forward * agentRadius * 2;
            Vector3 endPos = startPos - normal * Vector3.back * maxJumpDist * 1.1f;
            // Cheat forward a little bit so the sphereCast doesn't touch this ledge.
            Vector3 cheatStartPos = LerpByDistance(startPos, endPos, cheatOffset);
            //Debug.DrawRay(endPos, Vector3.up, Color.blue, 2);
            //Debug.DrawLine ( cheatStartPos , endPos, Color.white, debugLineDuration);
            //Debug.DrawLine(startPos, endPos, Color.white, debugLineDuration);


            RaycastHit raycastHit = new RaycastHit();

            //calculate direction for Spherecast
            ReUsableV3 = endPos - startPos;
            // raise up pos Y value slightly up to check for wall/obstacle
            offSetPosY = new Vector3(pos.x, (pos.y + wallCheckYOffset), pos.z);
            // ray cast to check for walls
            if (!Physics.Raycast(offSetPosY, ReUsableV3, (maxJumpDist / 2), raycastLayerMask.value))
            {
                //Debug.DrawRay(offSetPosY, ReUsableV3, Color.yellow, 15);
                Vector3 ReverseRayCastSpot = (offSetPosY + (ReUsableV3));
                //now raycast back the other way to make sure we're not raycasting through the inside of a mesh the first time.
                if (!Physics.Raycast(ReverseRayCastSpot, -ReUsableV3, (maxJumpDist + 1), raycastLayerMask.value))
                {
                    //Debug.DrawRay(ReverseRayCastSpot, -ReUsableV3, Color.red, 15);
                    //Debug.DrawRay(ReverseRayCastSpot, -ReUsableV3, Color.red, 15);

                    //if no walls 1 unit out then check for other colliders using the Cheat offset so as to not detect the edge we are spherecasting from.
                    if (Physics.SphereCast(cheatStartPos, sphereCastRadius, ReUsableV3, out raycastHit, maxJumpDist, raycastLayerMask.value, QueryTriggerInteraction.Ignore))
                        //if (Physics.Linecast(startPos, endPos, out raycastHit, raycastLayerMask.value, QueryTriggerInteraction.Ignore))
                    {
                        Vector3 cheatRaycastHit = LerpByDistance(raycastHit.point, endPos, .2f);

                        NNInfo nodeInfo = AstarPath.active.GetNearest(cheatRaycastHit, NNConstraint.Default);
                        Vector3 closestPos = nodeInfo.position;

                        if (nodeInfo.node != null)
                        {
                            //Debug.Log("Success");
                            //Debug.DrawLine( pos, navMeshHit.position, Color.black, debugLineDuration);

                            if (Vector3.Distance(pos, closestPos) > 1.1f)
                            {
                                //SPAWN NAVMESH LINKS
                                Transform spawnedTransf = Instantiate(
                                    oneWayLinkPrefab.transform,
                                    pos - normal * Vector3.forward * 0.02f,
                                    normal
                                ) as Transform;

                                NodeLink2 nodeLink = spawnedTransf.GetComponent<NodeLink2>();
                                GameObject endPoint = new GameObject("end");
                                endPoint.transform.position = closestPos;
                                endPoint.transform.SetParent(spawnedTransf, true);
                                nodeLink.end = endPoint.transform;

                                spawnedTransf.SetParent(transform);
                                nodeLinks.Add(nodeLink);
                            }
                        }
                    }
                }
            }

            return result;
        }


        #endregion

        //Just a helper function I added to calculate a point between normalized distance of two V3s
        public Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
        {
            Vector3 P = x * Vector3.Normalize(B - A) + A;
            return P;
        }


        #region EdgeGen


        float triggerAngle = 0.999f;

        private void CalcEdges(NavGraph graph)
        {
            List<Vector3> edgepoints = GraphUtilities.GetContours(graph);
            Debug.Log("Got contours");

            for (int i = 0; i < edgepoints.Count - 1; i += 2)
            {
                //CALC FROM MESH OPEN EDGES vertices
                TrisToEdge(edgepoints[i], edgepoints[i + 1]);
            }

            foreach (Edge edge in edges)
            {
                //EDGE LENGTH
                edge.length = Vector3.Distance(
                    edge.start,
                    edge.end
                );
                
                //FACING NORMAL
                if (!edge.facingNormalCalculated)
                {
                    edge.facingNormal = Quaternion.LookRotation(Vector3.Cross(edge.end - edge.start, Vector3.up));


                    //Seems like the below is never entereed because edge.startUp is never set...
                    if (edge.startUp.sqrMagnitude > 0)
                    {
                        var vect = Vector3.Lerp(edge.endUp, edge.startUp, 0.5f) - Vector3.Lerp(edge.end, edge.start, 0.5f);
                        edge.facingNormal = Quaternion.LookRotation(Vector3.Cross(edge.end - edge.start, vect));
                        
                        //FIX FOR NORMALs POINTING DIRECT TO UP/DOWN
                        if (Mathf.Abs(Vector3.Dot(Vector3.up, (edge.facingNormal * Vector3.forward).normalized)) > triggerAngle)
                        {
                            edge.startUp += new Vector3(0, 0.1f, 0);
                            vect = Vector3.Lerp(edge.endUp, edge.startUp, 0.5f) -
                                   Vector3.Lerp(edge.end, edge.start, 0.5f);
                            edge.facingNormal = Quaternion.LookRotation(Vector3.Cross(edge.end - edge.start, vect));
                        }
                    }
                    
                    if (dontAllignYAxis)
                    {
                        edge.facingNormal = Quaternion.LookRotation(
                            edge.facingNormal * Vector3.forward,
                            Quaternion.LookRotation(edge.end - edge.start) * Vector3.up
                        );
                    }

                    edge.facingNormalCalculated = true;
                }
                
                if (invertFacingNormal | graph is GridGraph)
                {
                    edge.facingNormal = Quaternion.Euler(Vector3.up * 180) * edge.facingNormal;
                }
            }
        }



        private void TrisToEdge(Vector3 val1, Vector3 val2)
        {
            if (val1 == val2)
                return;

            Edge newEdge = new Edge(val1, val2);

            //remove duplicate edges
            foreach (Edge edge in edges)
            {
                if (edge.start == val1 & edge.end == val2 || edge.start == val2 & edge.end == val1)
                {
                    edges.Remove(edge);
                    return;
                }
            }

            edges.Add(newEdge);
        }
        
        #endregion
    }



    [Serializable]
    public class Edge
    {
        public Vector3 start;
        public Vector3 end;

        public Vector3 startUp;
        public Vector3 endUp;

        public float length;
        public Quaternion facingNormal;
        public bool facingNormalCalculated = false;


        public Edge(Vector3 startPoint, Vector3 endPoint)
        {
            start = startPoint;
            end = endPoint;
        }
    }

    



#if UNITY_EDITOR

    [CustomEditor(typeof(NavMeshLinks_AutoPlacer))]
    [CanEditMultipleObjects]
    public class NavMeshLinks_AutoPlacer_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinks_AutoPlacer) targ).Generate();
                }
            }

            if (GUILayout.Button("ClearLinks"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinks_AutoPlacer) targ).ClearLinks();
                }
            }
        }
    }

#endif
}
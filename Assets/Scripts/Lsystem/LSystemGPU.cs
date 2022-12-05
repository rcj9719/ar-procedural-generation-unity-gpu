using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
//using UnityEngine.Scripting;
//using System.Runtime.ConstrainedExecution;
//using Unity.VisualScripting;
//using Unity.VisualScripting.Dependencies.Sqlite;

[Serializable]
public struct GPURules
{
    public char predecessor;
    public string successor;
}

public struct MeshData
{
    public Mesh mesh;
    public Material mat;
    public string id;
    public GameObject referenceToObj;

    /*public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public int[] triangles;*/
}

[Serializable]
public class LSystemGPU : MonoBehaviour
{
    public ComputeShader computeShader;
    public ComputeShader scanCS;

    private ComputeBuffer axiomBuffer;
    private ComputeBuffer scanBuffer;
    private ComputeBuffer scanInputBuffer;
    private ComputeBuffer resultBuffer;
    private ComputeBuffer auxBuffer;
    private ComputeBuffer deriveBuffer;
    private ComputeBuffer scanResBuffer;
    private ComputeBuffer scanCorBuffer;

    private ComputeBuffer posBuffer;
    private ComputeBuffer oriBuffer;
    private ComputeBuffer linkedBuffer;
    private ComputeBuffer depthBuffer;

    //[RequiredMember]
    public string axiom;

    public bool ProceduralInstantiate = false;
    public bool randomSize = false;

    [SerializeField]
    [Range(1, 5)]
    public int generations = 1;

    public List<GPURules> rules = new List<GPURules>();
    public List<GameObject> meshPrefabs = new List<GameObject>();

    private string primaryLettersChar;
    private string derivedLettersChar;

    private int letterSize;
    private int[] axiomLetters;
    private int[] primaryLetters;
    private int[] primaryLetterIndices;
    private int[] derivedLetters;

    private int[] scanBufferCorCPU;
    private int[] preScanResBuffer;

    private int[] afterScanResBuffer;
    private int[] derivedResBuffer;

    private List<Vector3> startPosList = new List<Vector3>();
    private List<Vector3> endPosList = new List<Vector3>();
    private List<Vector3> orientationList = new List<Vector3>();
    private List<String> meshNameData = new List<string>();
    private Dictionary<String, MeshData> meshPosData = new Dictionary<string, MeshData>();

    List<int> symbolList = new List<int>{70, 65,66, 72,74,75,77, 78, 79, 80, 82, 83, 84};
    int[] scanDepthRes;
    int[] linkRes;
    private int3[] posData;
    private int3[] oriData;


    // Kernel Handles
    int kernelInput;
    int kernelDerivation;
    int kernelDepth;
    int kernelLink;
    int scanInBucketKernel;
    int scanBucketResultKernel;
    int scanAddBucketResultKernel;

    #region LSystem
    void readRules()
    {
        letterSize = axiom.Length;

        primaryLettersChar = "";    // all predecessors together
        derivedLettersChar = "";    // all successors together

        primaryLetterIndices = new int[rules.Count * 4];    // assuming predecessor only has 1 character in every rule

        for (int i = 0; i < rules.Count; i++)
        {
            primaryLettersChar += rules[i].predecessor;
            derivedLettersChar += rules[i].successor;
            primaryLetterIndices[i * 4] = (i == 0) ? 0 : primaryLetterIndices[i - 1] + rules[i - 1].successor.Length;
        }
    }

    int charToAscii(char a)
    {
        //int sum = 70;
        switch (a)
        {
            case '+':
                return 43;
            case '-':
                return 45;
            case '*':
                return 42;
            case '/':
                return 47;
            case '&':
                return 38;
            case '^':
                return 94;
            case '[':
                return 91;
            case ']':
                return 93;
            case '|':
                return 124;
            case '\'':
                return 92;
            case 'F':
                return 70;
            case 'A':
                return 65;
            case 'B':
                return 66;
            case 'H':
                return 72;
            case 'J':
                return 74;
            case 'K':
                return 75;
            case 'M':
                return 77;
            case '%':    // % instead of +
                return 37;
            case '$':    // $ instead of -
                return 36;
            case '!':    // ! instead of division(/)
                return 33;
            case '=':    // = instead of &
                return 61;
            default:
                return 70;

        }
        //return sum;
    }

    string asciiToString(int num)
    {
        switch (num)
        {
            case 70:
                return "F";
            case 72:
                return "H";
            case 74:
                return "J";
            case 75:
                return "K";
            case 77:
                return "M";
            default:
                return "F";
        }
    }

    //void Derive() // the below function was previously called this
    //convert string to int[] so it can be passed to compute shader
    void convertRulesToAscii()
    {
        axiomLetters = new int[axiom.Length];
        primaryLetters = new int[primaryLettersChar.Length * 4];
        derivedLetters = new int[derivedLettersChar.Length * 4];
        int idx = 0;
        foreach (char a in primaryLettersChar)
        {
            primaryLetters[idx] = charToAscii(a);
            idx += 4;
        }
        idx = 0;
        foreach (char b in derivedLettersChar)
        {
            derivedLetters[idx] = charToAscii(b);
            idx += 4;
        }
        idx = 0;
        foreach (char c in axiom)
        {
            axiomLetters[idx] = charToAscii(c);
            idx++;
        }
    }

    //void UpdateOnGpu() // the below function was previously called this
    void setDataOnGPU()
    {
        // get GPU handles for device memory
        int primaryId = Shader.PropertyToID("_primaryLetters"),
            primarySizeId = Shader.PropertyToID("_primaryLetterSize"),
            derivedId = Shader.PropertyToID("_derivedLetters"),
            derivedSizeId = Shader.PropertyToID("_derivedLetterSize"),
            primaryIdxId = Shader.PropertyToID("_primaryLettersIdx"),
            primaryIdxSizeId = Shader.PropertyToID("_primaryLettersIdxSize"),
            preScanId = Shader.PropertyToID("_preScanRes"),
            stringLengthId = Shader.PropertyToID("_stringLength");

        // set device data
        computeShader.SetInt(stringLengthId, letterSize);
        //computeShader.SetInts(axiomId, axiomLetters);
        computeShader.SetInts(primaryId, primaryLetters);
        computeShader.SetInts(primaryIdxId, primaryLetterIndices);
        computeShader.SetInts(derivedId, derivedLetters);
        computeShader.SetInts(preScanId, preScanResBuffer);
        computeShader.SetInts(primarySizeId, primaryLetters.Length/4);
        computeShader.SetInts(derivedSizeId, derivedLetters.Length/4);
        computeShader.SetInts(primaryIdxSizeId, primaryLetterIndices.Length/4);
    }

    // Returns length of derived string
    int derive()
    {
        convertRulesToAscii();
        setDataOnGPU();
        int derlen = derivedLettersChar.Length;
        int priLen = primaryLettersChar.Length;
        // Pass 1 - COUNT - Count required number of output module letters
        for (int i = 0; i < generations; i++)
        {
            scanBufferCorCPU = new int[letterSize];
            preScanResBuffer = new int[letterSize];
            afterScanResBuffer = new int[letterSize];

            axiomBuffer = new ComputeBuffer(letterSize, sizeof(int));
            axiomBuffer.SetData(axiomLetters);
            scanBuffer = new ComputeBuffer(letterSize, sizeof(int));
            scanBuffer.SetData(preScanResBuffer);
            scanCorBuffer = new ComputeBuffer(letterSize, sizeof(int));
            scanCorBuffer.SetData(scanBufferCorCPU);
            computeShader.SetBuffer(kernelInput, "_axiomLetters", axiomBuffer);
            computeShader.SetBuffer(kernelInput, "_preScanCorBuffer", scanCorBuffer);
            computeShader.SetBuffer(kernelInput, "_preScanResBuffer", scanBuffer);
            computeShader.Dispatch(kernelInput, letterSize, 1, 1);

            scanBuffer.GetData(preScanResBuffer);
            scanCorBuffer.GetData(scanBufferCorCPU);

            // Pass 2 - SCAN - now scanBuffer contains an array of each derivation, we will do a inclusive sum scan on this array
            afterScanResBuffer = Scan(preScanResBuffer, letterSize, 0);

            int derivedSize = afterScanResBuffer[afterScanResBuffer.Length - 1];
            //Debug.Log(derivedSize);
            if (derivedSize <= 0) break;
            //Debug.Log("iter" + derivedSize + " " + afterScanResBuffer.Length);
            deriveBuffer = new ComputeBuffer(derivedSize, sizeof(int));
            scanResBuffer = new ComputeBuffer(letterSize, sizeof(int));
            scanResBuffer.SetData(afterScanResBuffer);
            computeShader.SetBuffer(kernelDerivation, "_afterScanResBuffer", scanResBuffer);
            computeShader.SetBuffer(kernelDerivation, "_preScanCorBuffer", scanCorBuffer);

            // Pass 3 - REWRITE  

            //Now after scan we have a inclusive scanned array, we can use that to populate an new derived array
            computeShader.SetBuffer(kernelDerivation, "_DeriveBuffer", deriveBuffer);
            computeShader.SetBuffer(kernelDerivation, "_axiomLetters", axiomBuffer);
            computeShader.Dispatch(kernelDerivation, letterSize, 1, 1);

            //now derivedResBuffer should have the new string;
            derivedResBuffer = new int[derivedSize];
            axiomLetters = new int[derivedSize];
            deriveBuffer.GetData(derivedResBuffer);
            deriveBuffer.GetData(axiomLetters);

            letterSize = derivedSize;
            axiomBuffer.Release();
            scanBuffer.Release();
            scanCorBuffer.Release();
            //deriveBuffer.Release();
            scanResBuffer.Release();


        }
        return letterSize;
    }

    #region rotate
    Vector3 rotateX(Vector3 v, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float ty = v.y;
        float tz = v.z;
        v.y = (cos * ty) - (sin * tz);
        v.z = (cos * tz) + (sin * ty);
        return v;
    }

    Vector3 rotateY(Vector3 v, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float tx = v.x;
        float tz = v.z;
        v.x = (cos * tx) + (sin * tz);
        v.z = (cos * tz) - (sin * tx);
        return v;
    }

    Vector3 rotateZ(Vector3 v, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (cos * ty) + (sin * tx);
        return v;
    }

    Vector3 rotateBranch(Vector3 pos, int3 ori)
    {
        pos = rotateX(pos, ori.x * (float)Math.PI / 180);
        pos = rotateY(pos, ori.y * (float)Math.PI / 180);
        pos = rotateZ(pos, ori.z * (float)Math.PI / 180);

        return pos;
    }
    #endregion

    void interpret(int derivedSize)
    {
        // Pass 1 - DEPTH CALCULATION

        //Setup:Depth Array/ Pos/Ori Array/ Linked List Array / derivedResBuffer
        depthBuffer = new ComputeBuffer(derivedSize, sizeof(int));
        posBuffer = new ComputeBuffer(derivedSize, sizeof(int) * 3);
        oriBuffer = new ComputeBuffer(derivedSize, sizeof(int) * 3);
        linkedBuffer = new ComputeBuffer(derivedSize, sizeof(int));

        computeShader.SetBuffer(kernelDepth, "_DeriveBuffer", deriveBuffer);
        computeShader.SetBuffer(kernelDepth, "_depthBuffer", depthBuffer);
        computeShader.SetBuffer(kernelDepth, "_posBuffer", posBuffer);
        computeShader.SetBuffer(kernelDepth, "_oriBuffer", oriBuffer);
        computeShader.SetBuffer(kernelLink, "_linkedBuffer", linkedBuffer);

        //first we will store each character's depth in an array, and in this process we will record any position/orientation
        computeShader.Dispatch(kernelDepth, derivedSize, 1, 1);
        int[] depthRes = new int[derivedSize];
        scanDepthRes = new int[derivedSize];
        depthBuffer.GetData(depthRes);


        // Pass 2 - DEPTH SCAN - after that we will do a scan on the depth
        scanDepthRes = Scan(depthRes, derivedSize, 1);
        depthBuffer.SetData(scanDepthRes);


        // Pass 3 - LINK BY DEPTH - we will fill a linked list array where every character record its parent/predecessor
        // It takes O(n) for each thread so it will take O(n) in the end!
        linkRes = new int[derivedSize];
        computeShader.SetBuffer(kernelLink, "_depthBuffer", depthBuffer);
        computeShader.Dispatch(kernelLink, derivedSize, 1, 1);

        linkedBuffer.GetData(linkRes);
        posData = new int3[derivedSize];
        oriData = new int3[derivedSize];

        posBuffer.GetData(posData);
        oriBuffer.GetData(oriData);

    }
    #endregion

    
    void getDrawPositions(int derivedSize)
    {
        List<Vector3> posDataFloat = new List<Vector3>();
        for(int i = 0; i < posData.Length; i++)
        {
            posDataFloat.Add(new Vector3(posData[i].x, posData[i].y, posData[i].z));
        }

        for (int i = 0; i < derivedSize; i++)
        {
            int cur = derivedResBuffer[i];

            if (i == 0 && (symbolList.Contains(cur)))
            {
                startPosList.Add(Vector3.zero);
                endPosList.Add(posDataFloat[i]);
                orientationList.Add(new Vector3(oriData[i].x, oriData[i].y, oriData[i].z));
                meshNameData.Add(asciiToString(cur));
                continue;
            }

            int prevIdx = linkRes[i];
            oriData[i] += oriData[prevIdx];
            if (symbolList.Contains(cur))
            {
                Vector3 startPos = posDataFloat[prevIdx];
                Vector3 endPos = new Vector3();

                Vector3 localPos = posDataFloat[i];
                Vector3 localPosRotated = rotateBranch(localPos, oriData[i]);

                endPos = localPosRotated + startPos;
                posDataFloat[i] = endPos;
                startPosList.Add(startPos);
                endPosList.Add(endPos);
                orientationList.Add(new Vector3(oriData[i].x, oriData[i].y, oriData[i].z));
                meshNameData.Add(asciiToString(cur));
            }
            else
            {
                posDataFloat[i] += posDataFloat[prevIdx];
            }
        }
    }

    void prepareMeshData(List<string> meshName)
    {
        foreach (GameObject prefab1 in meshPrefabs)
        {
            MeshData newData = new MeshData();
            Mesh mesh1 = prefab1.GetComponent<MeshFilter>().sharedMesh;
            newData.mesh = mesh1;
            newData.id = prefab1.name;
            newData.referenceToObj = prefab1;
            newData.mat = prefab1.GetComponent<MeshRenderer>().sharedMaterial;
            meshPosData.Add(newData.id, newData);
        }
    }

    void drawLines()
    {
        int currentLine = 0;
        Material green = (Material)Resources.Load("Green", typeof(Material));
        
        for (int i = 0; i < startPosList.Count; i++)
        {
            var lineGo = new GameObject($"Line_{currentLine}");
            lineGo.transform.position = Vector3.zero;
            lineGo.transform.parent = transform;

            LineRenderer newLine = lineGo.AddComponent<LineRenderer>();
            newLine.positionCount = 2;
            newLine.startWidth = 0.05f;
            newLine.endWidth = 0.05f;
            newLine.startColor = Color.green;
            newLine.endColor = Color.green;
            newLine.useWorldSpace = true;
            newLine.positionCount = 2;
            newLine.material = green;

            newLine.SetPosition(0, this.transform.position + startPosList[i]);
            newLine.SetPosition(1, this.transform.position + endPosList[i]);
            currentLine++;
        }
    }

    void drawInstantiate()
    {
        //Debug.Log(meshNameData.Count);
        for(int i = 0; i < meshNameData.Count; i++) // for each characer in derived string
        {
            string curChar = meshNameData[i];
            MeshData meshD = meshPosData[meshNameData[i]];
            //Debug.Log(startPosList[i].x + " " + startPosList[i].y + " " + startPosList[i].z);
            //Debug.Log(endPosList[i].x + " " + endPosList[i].y + " " + endPosList[i].z);
            Quaternion qua = Quaternion.FromToRotation(endPosList[0] - startPosList[0], endPosList[i] - startPosList[i]);
            GameObject newObj = Instantiate(meshD.referenceToObj, this.transform.position + (endPosList[i] - startPosList[i])/2 + startPosList[i] , qua);
            newObj.transform.parent = this.transform;

            //scaling part for AR goes downwards
        }
        this.transform.localScale = this.transform.localScale * 0.1f;
        if (randomSize)
        {
            this.transform.localScale *= UnityEngine.Random.Range(0.5f, 2.0f);
        }
    }
    void drawProcedural()
    {
        for (int i = 0; i < meshNameData.Count; i++)
        {
            //Debug.Log("drawing");
            //Debug.Log(i);
            MeshData meshD = meshPosData[meshNameData[i]];
            Quaternion qua = Quaternion.FromToRotation(endPosList[0] - startPosList[0], endPosList[i] - startPosList[i]);
            //Debug.Log(materials[0]);
            Graphics.DrawMesh(meshD.mesh, startPosList[i] + (endPosList[i] - startPosList[i]) / 2 + this.transform.position, qua, meshD.mat, 0);
        }
    }

    //private void OnEnable() { 
    void Awake() {
        //// get compute kernel handles
        //kernelInput = computeShader.FindKernel("Input");
        //kernelDerivation = computeShader.FindKernel("Derivation");
        //kernelDepth = computeShader.FindKernel("calculateDepth");
        //kernelLink = computeShader.FindKernel("linkArray");

        /*//Debug.Log(Time.deltaTime);
        //// Reads rules from inspector and sets required string buffers
        readRules();
        int derivedSize = derive();
        //Debug.Log(derivedSize);
        //With the new string, we will start on working the interpretation part.
        interpret(derivedSize);

        getDrawPositions(derivedSize);
        draw();

        destroy();
        //Debug.Log(Time.deltaTime);*/
    }

    private void Start()
    {
        // get compute kernel handles
        kernelInput = computeShader.FindKernel("Input");
        kernelDerivation = computeShader.FindKernel("Derivation");
        kernelDepth = computeShader.FindKernel("calculateDepth");
        kernelLink = computeShader.FindKernel("linkArray");
        Debug.Log("Time started for L-system generation " + Time.realtimeSinceStartup);
        // Reads rules from inspector and sets required string buffers
        readRules();
        int derivedSize = derive();

        if (derivedSize == 0)
        {
            Debug.LogError("Derivation scan failed. Cannot process lsystem");
        }

        //With the new string, we will start on working the interpretation part.
        interpret(derivedSize);
        getDrawPositions(derivedSize);
        prepareMeshData(meshNameData);
        //draw();
        if (!ProceduralInstantiate)
        {
            drawInstantiate();
        }

        destroy();
        Debug.Log("Time ended for L-system generation " + Time.realtimeSinceStartup);
    }

    

    void Update()
    {
        //assume now we have startPos and endPos, along with the meshNameData, each with corresponding index; Also we have the meshDataMap which each string leads to a different meshData

        //the current idea is to for every mesh that we use, we will use Graphics.DrawMesh to draw it to the screen
        //Debug.Log("start " +Time.realtimeSinceStartup);
        if (ProceduralInstantiate)
        {
            drawProcedural();
        }
        //Debug.Log("end " + Time.realtimeSinceStartup);

    }

    private void destroy()
    {
        axiomBuffer.Release();
        deriveBuffer.Release();
        resultBuffer.Release();
        scanBuffer.Release();
        scanInputBuffer.Release();
        auxBuffer.Release();
        scanResBuffer.Release();
        scanCorBuffer.Release();

        posBuffer.Release();
        oriBuffer.Release();
        linkedBuffer.Release();
        depthBuffer.Release();
    }

    int[] Scan(int[] input, int letterSize, int c)
    {
        int scanInBucketKernel = scanCS.FindKernel("ScanInBucketInclusive");
        int scanBucketResultKernel = scanCS.FindKernel("ScanBucketResult");
        int scanAddBucketResultKernel = scanCS.FindKernel("ScanAddBucketResult");
        int[] output = new int[letterSize];

        int[] output_tempres1 = new int[letterSize];
        int[] output_tempaaux1 = new int[letterSize];
        int[] output_tempres2 = new int[letterSize];
        int[] output_tempaux2 = new int[letterSize];
        int[] output_tempaux3 = new int[letterSize];

        int threads_per_group = 512;

        //int threadGroupCount = letterSize / threads_per_group == 0 ? 1 : letterSize / threads_per_group;
        int threadGroupCount = (letterSize / threads_per_group) + 1;
        //int threadGroupCount = letterSize/ threads_per_group == 0 ? letterSize : (letterSize/threads_per_group) + 1;

        scanInputBuffer = new ComputeBuffer(letterSize, sizeof(int));
        scanInputBuffer.SetData(input);
        resultBuffer = new ComputeBuffer(letterSize, sizeof(int));
        auxBuffer = new ComputeBuffer(letterSize, sizeof(int));

        // ScanInBucket.
        scanCS.SetBuffer(scanInBucketKernel, "_Input", scanInputBuffer);
        scanCS.SetBuffer(scanInBucketKernel, "_Result", resultBuffer);
        scanCS.Dispatch(scanInBucketKernel, threadGroupCount, 1, 1);

        
        resultBuffer.GetData(output_tempres1);
        auxBuffer.GetData(output_tempaaux1);

        // ScanBucketResult.
        scanCS.SetBuffer(scanBucketResultKernel, "_Input", resultBuffer);
        scanCS.SetBuffer(scanBucketResultKernel, "_Result", auxBuffer);
        scanCS.Dispatch(scanBucketResultKernel, 1, 1, 1);

        resultBuffer.GetData(output_tempres2);
        auxBuffer.GetData(output_tempaux2);

        // ScanAddBucketResult.
        scanCS.SetBuffer(scanAddBucketResultKernel, "_Input", auxBuffer);
        scanCS.SetBuffer(scanAddBucketResultKernel, "_Result", resultBuffer);
        scanCS.Dispatch(scanAddBucketResultKernel, threadGroupCount, 1, 1);
        

        resultBuffer.GetData(output);

        //Debug.LogError("#######################################" + output[0]);
        //Debug.LogError("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&" + output[1]);
        //auxBuffer.GetData(output_tempaux3);
        scanInputBuffer.Release();
        resultBuffer.Release();
        auxBuffer.Release();

        return output;
        //if (c == 0) return output_tempres1;
        //else return output;
    }

    public void getLines(ref List<Vector3> startPositions, ref List<Vector3> endPositions)
    {
        startPositions.AddRange(startPosList);
        endPositions.AddRange(endPosList);
    }
}
// Experimental Direct3D 12 shader contract.
// The real renderer will replace the two-bone constant matrices with a bone-matrix SRV.
cbuffer SceneConstants : register(b0)
{
    row_major float4x4 ViewProjection;
    row_major float4x4 Bone0;
    row_major float4x4 Bone1;
    float4 BaseColor;
    float NormalLength;
    float3 Padding;
};

struct VSInput
{
    float3 Position    : POSITION;
    float3 Normal      : NORMAL;
    uint4  BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};

struct RasterVertex
{
    float4 Position      : SV_POSITION;
    float3 ModelPosition : TEXCOORD0;
    float3 Normal        : TEXCOORD1;
    float4 Color         : COLOR0;
};

RasterVertex VSMain(VSInput input)
{
    RasterVertex output;
    float4 positionSum = 0.0f;
    float3 normalSum = 0.0f;
    float weightSum = 0.0f;

    [unroll]
    for (int influence = 0; influence < 4; ++influence)
    {
        float weight = max(0.0f, input.BoneWeights[influence]);
        if (weight <= 0.00001f) continue;

        // This prototype deliberately contains two bones. The production backend will use
        // a structured buffer so P1/P2 skeletons are not limited to two joints.
        uint boneIndex = input.BoneIndices[influence];
        if (boneIndex == 0)
        {
            positionSum += mul(float4(input.Position, 1.0f), Bone0) * weight;
            normalSum += mul(input.Normal, (float3x3)Bone0) * weight;
        }
        else
        {
            positionSum += mul(float4(input.Position, 1.0f), Bone1) * weight;
            normalSum += mul(input.Normal, (float3x3)Bone1) * weight;
        }
        weightSum += weight;
    }

    if (weightSum <= 0.00001f)
    {
        positionSum = float4(input.Position, 1.0f);
        normalSum = input.Normal;
    }
    else
    {
        positionSum /= weightSum;
        normalSum /= weightSum;
    }

    output.ModelPosition = positionSum.xyz;
    output.Normal = normalize(normalSum);
    output.Position = mul(float4(output.ModelPosition, 1.0f), ViewProjection);
    output.Color = BaseColor;
    return output;
}

float4 PSMain(RasterVertex input) : SV_TARGET
{
    float3 lightDirection = normalize(float3(0.35f, 0.72f, 0.58f));
    float diffuse = 0.28f + 0.72f * saturate(dot(normalize(input.Normal), lightDirection));
    return float4(input.Color.rgb * diffuse, input.Color.a);
}

// GPU-side diagnostic overlay: normals become line primitives in a geometry shader.
// No CPU-generated tube meshes or per-frame WPF MeshGeometry3D collections are required.
[maxvertexcount(6)]
void GSNormals(triangle RasterVertex input[3], inout LineStream<RasterVertex> stream)
{
    [unroll]
    for (int i = 0; i < 3; ++i)
    {
        float3 normal = normalize(input[i].Normal);
        float4 normalColor = float4(0.35f + 0.65f * abs(normal), 1.0f);

        RasterVertex startVertex = input[i];
        startVertex.Color = normalColor;
        stream.Append(startVertex);

        RasterVertex endVertex = input[i];
        endVertex.ModelPosition = input[i].ModelPosition + normal * NormalLength;
        endVertex.Position = mul(float4(endVertex.ModelPosition, 1.0f), ViewProjection);
        endVertex.Color = normalColor;
        stream.Append(endVertex);
        stream.RestartStrip();
    }
}

cbuffer SceneConstants : register(b0)
{
    row_major float4x4 ViewProjection;
};

StructuredBuffer<uint> TexturePixels : register(t0);
struct BoneMatrixData
{
    row_major float4x4 Transform;
};
StructuredBuffer<BoneMatrixData> BoneMatrices : register(t1);

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color : COLOR0;
    uint4 TextureInfo : TEXINFO0;
    float3 RestPosition : RESTPOSITION0;
    uint4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
    uint IsSkinned : SKINFLAG0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
    float4 Color : COLOR0;
    nointerpolation uint4 TextureInfo : TEXINFO0;
    float3 RestPosition : TEXCOORD2;
    float3 DeformedPosition : TEXCOORD3;
};

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    float3 position = input.Position;
    float3 normal = input.Normal;

    // The CPU evaluates only the small joint hierarchy and uploads one matrix per bone.
    // All per-vertex position/normal blending now happens in the GPU vertex shader.
    if (input.IsSkinned != 0u)
    {
        float4 weights = max(input.BoneWeights, 0.0f);
        float weightSum = weights.x + weights.y + weights.z + weights.w;
        if (weightSum > 0.000001f)
        {
            weights /= weightSum;
            float4 bindPosition = float4(input.Position, 1.0f);
            float4 bindNormal = float4(input.Normal, 0.0f);
            float4 skinnedPosition = 0.0f;
            float4 skinnedNormal = 0.0f;

            if (weights.x > 0.0f)
            {
                row_major float4x4 bone = BoneMatrices[input.BoneIndices.x].Transform;
                skinnedPosition += mul(bindPosition, bone) * weights.x;
                skinnedNormal += mul(bindNormal, bone) * weights.x;
            }
            if (weights.y > 0.0f)
            {
                row_major float4x4 bone = BoneMatrices[input.BoneIndices.y].Transform;
                skinnedPosition += mul(bindPosition, bone) * weights.y;
                skinnedNormal += mul(bindNormal, bone) * weights.y;
            }
            if (weights.z > 0.0f)
            {
                row_major float4x4 bone = BoneMatrices[input.BoneIndices.z].Transform;
                skinnedPosition += mul(bindPosition, bone) * weights.z;
                skinnedNormal += mul(bindNormal, bone) * weights.z;
            }
            if (weights.w > 0.0f)
            {
                row_major float4x4 bone = BoneMatrices[input.BoneIndices.w].Transform;
                skinnedPosition += mul(bindPosition, bone) * weights.w;
                skinnedNormal += mul(bindNormal, bone) * weights.w;
            }

            position = skinnedPosition.xyz;
            normal = dot(skinnedNormal.xyz, skinnedNormal.xyz) > 0.000001f
                ? normalize(skinnedNormal.xyz) : input.Normal;
        }
    }

    output.Position = mul(float4(position, 1.0f), ViewProjection);
    output.Normal = normalize(normal);
    output.TextureCoordinate = input.TextureCoordinate;
    output.Color = input.Color;
    output.TextureInfo = input.TextureInfo;
    output.RestPosition = input.RestPosition;
    output.DeformedPosition = position;
    return output;
}

float3 Lighting(float3 normal)
{
    float3 lightDirection = normalize(float3(0.35f, 0.72f, 0.58f));
    float lighting = 0.28f + 0.72f * saturate(dot(normalize(normal), lightDirection));
    return float3(lighting, lighting, lighting);
}

float4 UnpackPixel(uint packed)
{
    return float4(
        (packed & 255u) / 255.0f,
        ((packed >> 8) & 255u) / 255.0f,
        ((packed >> 16) & 255u) / 255.0f,
        ((packed >> 24) & 255u) / 255.0f);
}

uint WrapIndex(int coordinate, uint size)
{
    int s = max(1, (int)size);
    int value = coordinate % s;
    if (value < 0) value += s;
    return (uint)value;
}

float4 SamplePreviewTexture(float2 uv, uint4 textureInfo)
{
    if (textureInfo.w == 0u || textureInfo.y == 0u || textureInfo.z == 0u)
        return float4(1, 1, 1, 1);

    float2 wrappedUv = frac(uv);
    float2 samplePosition = wrappedUv * float2(textureInfo.y, textureInfo.z) - 0.5f;
    int2 p0 = (int2)floor(samplePosition);
    int2 p1 = p0 + int2(1, 1);
    float2 blend = frac(samplePosition);

    uint x0 = WrapIndex(p0.x, textureInfo.y);
    uint x1 = WrapIndex(p1.x, textureInfo.y);
    uint y0 = WrapIndex(p0.y, textureInfo.z);
    uint y1 = WrapIndex(p1.y, textureInfo.z);
    uint baseOffset = textureInfo.x;

    float4 c00 = UnpackPixel(TexturePixels[baseOffset + y0 * textureInfo.y + x0]);
    float4 c10 = UnpackPixel(TexturePixels[baseOffset + y0 * textureInfo.y + x1]);
    float4 c01 = UnpackPixel(TexturePixels[baseOffset + y1 * textureInfo.y + x0]);
    float4 c11 = UnpackPixel(TexturePixels[baseOffset + y1 * textureInfo.y + x1]);
    return lerp(lerp(c00, c10, blend.x), lerp(c01, c11, blend.x), blend.y);
}

float4 PSSolid(VSOutput input) : SV_TARGET
{
    return float4(input.Color.rgb * Lighting(input.Normal), input.Color.a);
}

float4 PSTextured(VSOutput input) : SV_TARGET
{
    float4 textureColor = SamplePreviewTexture(input.TextureCoordinate, input.TextureInfo);
    return float4(textureColor.rgb * input.Color.rgb * Lighting(input.Normal), textureColor.a * input.Color.a);
}

float4 PSNormals(VSOutput input) : SV_TARGET
{
    float3 n = normalize(input.Normal);
    return float4(n * 0.5f + 0.5f, 1.0f);
}

float4 PSSkinning(VSOutput input) : SV_TARGET
{
    return float4(input.Color.rgb, 1.0f);
}

struct DeformationOutput
{
    float4 Position : SV_POSITION;
    nointerpolation float4 DeformationColor : COLOR0;
};

[maxvertexcount(3)]
void GSDeformation(triangle VSOutput input[3], inout TriangleStream<DeformationOutput> stream)
{
    float3 restA = input[1].RestPosition - input[0].RestPosition;
    float3 restB = input[2].RestPosition - input[0].RestPosition;
    float3 currentA = input[1].DeformedPosition - input[0].DeformedPosition;
    float3 currentB = input[2].DeformedPosition - input[0].DeformedPosition;
    float3 restCross = cross(restA, restB);
    float3 currentCross = cross(currentA, currentB);
    float restDoubleArea = length(restCross);
    float currentDoubleArea = length(currentCross);
    float ratio = currentDoubleArea / max(restDoubleArea, 1.0e-7f);

    float4 color;
    if (restDoubleArea < 1.0e-7f)
    {
        color = float4(0.95f, 0.82f, 0.16f, 1.0f); // Degenerate in bind pose.
    }
    else if (dot(restCross, currentCross) < 0.0f)
    {
        color = float4(0.88f, 0.16f, 0.92f, 1.0f); // Possible triangle inversion.
    }
    else
    {
        float delta = log2(max(ratio, 1.0e-4f));
        if (delta < -0.025f)
        {
            float amount = saturate((-delta) * 1.6f);
            color = lerp(float4(0.18f, 0.82f, 0.42f, 1.0f), float4(0.08f, 0.36f, 1.0f, 1.0f), amount);
        }
        else if (delta > 0.025f)
        {
            float amount = saturate(delta * 1.6f);
            color = lerp(float4(0.18f, 0.82f, 0.42f, 1.0f), float4(1.0f, 0.20f, 0.10f, 1.0f), amount);
        }
        else
        {
            color = float4(0.18f, 0.82f, 0.42f, 1.0f); // Stable surface area.
        }
    }

    [unroll]
    for (uint vertex = 0; vertex < 3; vertex++)
    {
        DeformationOutput output;
        output.Position = input[vertex].Position;
        output.DeformationColor = color;
        stream.Append(output);
    }
    stream.RestartStrip();
}

float4 PSDeformation(DeformationOutput input) : SV_TARGET
{
    return input.DeformationColor;
}

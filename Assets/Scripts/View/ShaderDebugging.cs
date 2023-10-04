using System;
using UnityEngine;

public class ShaderDebugging : IDisposable
{
	public const int WIDTH = 50;
	public const int HEIGHT = 50;

	private GraphicsBuffer _rewardValueBuffer;
	private Vector2[] _position;
	private float[] _prevValue;
    private bool disposedValue;

    public ShaderDebugging(Renderer renderer, Bounds bounds)
	{
		_position = new Vector2[WIDTH*HEIGHT];
		_prevValue = new float[WIDTH*HEIGHT];
		_rewardValueBuffer = new GraphicsBuffer(
			GraphicsBuffer.Target.Structured,
			GraphicsBuffer.UsageFlags.LockBufferForWrite,
			WIDTH * HEIGHT,
			sizeof(float)
		);

		var material = renderer.material;
		material.SetBuffer("value", _rewardValueBuffer);
        material.SetInt("width", WIDTH);
		material.SetInt("height", HEIGHT);

		float gridSizeX = bounds.size.x/WIDTH;
		float gridSizeY = bounds.size.y/HEIGHT;
		for (int j = 0; j < HEIGHT; j++)
		{
			for (int i = 0; i < WIDTH; i++)
			{
				_position[(HEIGHT-1-j)*WIDTH+(WIDTH-1-i)] = new Vector3(gridSizeX*(i+0.5f-WIDTH/2), gridSizeY*(j+0.5f-HEIGHT/2), 0);
			}
		}
	}

    public void Update(RewardFunction reward)
	{
		var nativeArray = _rewardValueBuffer.LockBufferForWrite<float>(0, WIDTH*HEIGHT);
		for (int j = 0; j < HEIGHT; j++)
		{
			for (int i = 0; i < WIDTH; i++)
			{
				int idx = j*WIDTH+i;
				var worldPosition = _position[idx];
				var value = reward.GetVariableReward(worldPosition);
				// Normalize [-1, 1] to color space of [0, 1]
				//nativeArray[idx] = (value-_prevValue[idx])*20+0.7f;
				nativeArray[idx] = value*2+0.7f;
				_prevValue[idx] = value;
			}
		}
		_rewardValueBuffer.UnlockBufferAfterWrite<float>(WIDTH*HEIGHT);
	}

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _rewardValueBuffer.Release();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

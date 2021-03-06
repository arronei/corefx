// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ManualResetEventTests
{
    [Fact]
    public void Ctor()
    {
        using (ManualResetEvent mre = new ManualResetEvent(false))
            Assert.False(mre.WaitOne(0));

        using (ManualResetEvent mre = new ManualResetEvent(true))
            Assert.True(mre.WaitOne(0));
    }

    [Fact]
    public void SetReset()
    {
        using (ManualResetEvent mre = new ManualResetEvent(false))
        {
            Assert.False(mre.WaitOne(0));
            mre.Set();
            Assert.True(mre.WaitOne(0));
            Assert.True(mre.WaitOne(0));
            mre.Set();
            Assert.True(mre.WaitOne(0));
            mre.Reset();
            Assert.False(mre.WaitOne(0));
        }
    }

    [Fact]
    public void WaitHandleWaitAll()
    {
        ManualResetEvent[] handles = new ManualResetEvent[10];
        for (int i = 0; i < handles.Length; i++)
            handles[i] = new ManualResetEvent(false);

        Task<bool> t = Task.Run(() => WaitHandle.WaitAll(handles));
        for (int i = 0; i < handles.Length; i++)
        {
            Assert.False(t.IsCompleted);
            handles[i].Set();
        }
        Assert.True(t.Result);

        Assert.True(WaitHandle.WaitAll(handles, 0));
    }

    [Fact]
    public void WaitHandleWaitAny()
    {
        ManualResetEvent[] handles = new ManualResetEvent[10];
        for (int i = 0; i < handles.Length; i++)
            handles[i] = new ManualResetEvent(false);

        Task<int> t = Task.Run(() => WaitHandle.WaitAny(handles));
        handles[5].Set();
        Assert.Equal(5, t.Result);

        Assert.Equal(5, WaitHandle.WaitAny(handles, 0));
    }

    [Fact]
    public void PingPong()
    {
        using (ManualResetEvent mre1 = new ManualResetEvent(true), mre2 = new ManualResetEvent(false))
        {
            const int Iters = 10;
            Task.WaitAll(
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < Iters; i++)
                    {
                        Assert.True(mre1.WaitOne());
                        mre1.Reset();
                        mre2.Set();
                    }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default),
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < Iters; i++)
                    {
                        Assert.True(mre2.WaitOne());
                        mre2.Reset();
                        mre1.Set();
                    }
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default));
        }
    }

}

﻿using System;
using System.Collections.Concurrent;

class Types
{
    static object s_one = new object();
    static object s_two = new object();
    static object s_three = new object();

    static object[] s_array = new object[] { s_one, s_two, s_three };

    static Foo s_foo = new Foo();
    static ConcurrentQueue<int> s_cq = new ConcurrentQueue<int>();

    static object s_i = 42;

    static Types()
    {
    }

    public static void Main()
    {
        new StructTestClass(); // Ensure type is constructed
        Foo f = new Foo();
        Foo[] foos = new Foo[] { f };

        Inner();

        GC.KeepAlive(foos);
    }

    private static void Inner()
    {
        throw new Exception();
    }
}

using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class BaseTestClass : ITestStartEventReceiver, ITestEndEventReceiver
{
    public ValueTask OnTestEnd(TestContext context)
    {
        //var message = $"<<< Finished Test: {context.ClassContext.ClassType.Name}.{context.TestName}" + $"{Environment.NewLine}" + $"Result={context.Result.State}  :: {context.Result.Exception.Message}";

        //Debug.WriteLine(message);

        //File.AppendAllLines("C:\\temp\\text.log", [message]);

        return ValueTask.CompletedTask;
    }

    public ValueTask OnTestStart(TestContext context)
    {
        //var message = $">>> Starting Test: {context.ClassContext.ClassType.Name}.{context.TestName}";
        //Debug.WriteLine(message);

        //File.AppendAllLines("C:\\temp\\text.log", [message]);

        return ValueTask.CompletedTask;
    }
}

public class ServiceIntegrationTests
{
    [BeforeEvery(HookType.Class)]
    public static async Task BeforeClass()
    {
        var context = ClassHookContext.Current;


        File.AppendAllLines("C:\\temp\\text.log", [$"  >>> CLASS {context.ClassType.Name}"]);
    }

    [BeforeEvery(HookType.Test)]
    public static async Task BeforeTest()
    {
        var context = TestContext.Current;

        Debug.WriteLine($"*** Starting Test: {context.ClassContext.ClassType.Name}.{context.Metadata.TestName}");

        File.AppendAllLines("C:\\temp\\text.log", [$"    >>>  TEST {context.ClassContext.ClassType.Name}.{context.Metadata.TestName}"]);
    }

    [BeforeEvery(HookType.Assembly)]
    public static async Task BeforeAssembly()
    {
        var context = AssemblyHookContext.Current;

        File.AppendAllLines("C:\\temp\\text.log", [$"  >>>  ASSEMBLY {context.Assembly.GetName().Name}"]);
    }



    [AfterEvery(HookType.Class)]
    public static async Task AfterClass()
    {
        var context = ClassHookContext.Current;


        File.AppendAllLines("C:\\temp\\text.log", [$"  <<< CLASS {context.ClassType.Name}"]);
    }

    [AfterEvery(HookType.Test)]
    public static async Task AfterTest()
    {
        var context = TestContext.Current;

        var message1 = $"    <<<  TEST {context.ClassContext.ClassType.Name}.{context.Metadata.TestName}";
        var message2 = $"            Result={context.Execution.Result.State}";

        var messages = new string[] { message1, message2 };

        if (TestContext.Current.StateBag.ContainsKey("db_name"))
        {
            var message3 = $"            DB={TestContext.Current.StateBag["db_name"]}";
            messages = new string[] { message1, message3, message2 };
        }

        File.AppendAllLines("C:\\temp\\text.log", messages);
    }

    [AfterEvery(HookType.Assembly)]
    public static async Task AfterAssembly()
    {
        var context = AssemblyHookContext.Current;

        File.AppendAllLines("C:\\temp\\text.log", [$"  <<<  ASSEMBLY {context.Assembly.GetName().Name}"]);
    }
}
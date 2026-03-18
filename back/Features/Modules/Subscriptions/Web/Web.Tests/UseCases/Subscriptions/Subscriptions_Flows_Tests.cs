using System.Net;
using PaintingProjectsManagement.Features.Subscriptions;
using PaintingProjectsManagement.Testing.Core;

namespace PaintingProjectsManagement.Features.Subscriptions.Tests;

[HumanFriendlyDisplayName]
public class Subscriptions_Flows_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");

        using var context = TestingServer.CreateContext();
        await context.Set<SubscriptionPayment>().ExecuteDeleteAsync();
        await context.Set<TenantSubscription>().ExecuteDeleteAsync();
        await context.SaveChangesAsync();
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Current_Subscription()
    {
        var response = await TestingServer.GetAsync<CurrentSubscriptionDetails>("api/subscriptions/me");
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Can_Get_Current_Subscription()
    {
        var response = await TestingServer.GetAsync<CurrentSubscriptionDetails>("api/subscriptions/me", "rodrigo.basniak");

        response.ShouldBeSuccess(out var result);
        result.Tier.ShouldBe(SubscriptionTier.Free);
        result.Status.ShouldBe(SubscriptionStatus.Active);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Can_Subscribe_To_Basic()
    {
        var response = await TestingServer.PostAsync<CurrentSubscriptionDetails>(
            "api/subscriptions/subscribe",
            new Subscribe.Request
            {
                Tier = SubscriptionTier.Basic
            },
            "rodrigo.basniak");

        response.ShouldBeSuccess(out var result);
        result.Tier.ShouldBe(SubscriptionTier.Basic);
        result.MonthlyPriceUsd.ShouldBe(9m);

        using var context = TestingServer.CreateContext();
        var payment = context.Set<SubscriptionPayment>().FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK");
        payment.ShouldNotBeNull();
        payment.Status.ShouldBe(SubscriptionPaymentStatus.Succeeded);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Upgrade_To_Premium()
    {
        var response = await TestingServer.PostAsync<CurrentSubscriptionDetails>(
            "api/subscriptions/upgrade",
            new UpgradeSubscription.Request
            {
                Tier = SubscriptionTier.Premium
            },
            "rodrigo.basniak");

        response.ShouldBeSuccess(out var result);
        result.Tier.ShouldBe(SubscriptionTier.Premium);
        result.MonthlyPriceUsd.ShouldBe(29m);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Can_Cancel_At_Period_End()
    {
        var response = await TestingServer.PostAsync<CurrentSubscriptionDetails>(
            "api/subscriptions/cancel",
            new CancelSubscription.Request
            {
                CancelAtPeriodEnd = true
            },
            "rodrigo.basniak");

        response.ShouldBeSuccess(out var result);
        result.Tier.ShouldBe(SubscriptionTier.Premium);
        result.CancelAtPeriodEnd.ShouldBeTrue();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Cancel_Immediately()
    {
        var response = await TestingServer.PostAsync<CurrentSubscriptionDetails>(
            "api/subscriptions/cancel",
            new CancelSubscription.Request
            {
                CancelAtPeriodEnd = false
            },
            "rodrigo.basniak");

        response.ShouldBeSuccess(out var result);
        result.Tier.ShouldBe(SubscriptionTier.Free);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}

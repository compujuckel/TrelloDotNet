﻿using TrelloDotNet.Model.Webhook;

namespace TrelloDotNet.Tests.IntegrationTests;

public class WebhookManagementTests : TestBaseWithNewBoard
{
    [Fact]
    public async Task WebHookCrud()
    {
        //Find current webhooks
        var currentWebhooks = await TrelloClient.GetWebhooksForCurrentToken();

        //Add Webhook
        var callbackUrl = "https://www.google.com";
        var description = "Webhook1";
        var webhook = new Webhook(description, callbackUrl, BoardId);
        var addedWebHook = await TrelloClient.AddWebhookAsync(webhook);
        Assert.Equal(callbackUrl, addedWebHook.CallbackUrl);
        Assert.Equal(description, addedWebHook.Description);
        Assert.True(addedWebHook.Active);
        var webhookId = addedWebHook.Id;

        //Update Webhook
        var updatedDescription = "Webhook2";
        addedWebHook.Description = updatedDescription;
        addedWebHook.Active = false;
        var updatedWebHook = await TrelloClient.UpdateWebhookAsync(addedWebHook);
        Assert.Equal(callbackUrl, updatedWebHook.CallbackUrl);
        Assert.Equal(updatedDescription, updatedWebHook.Description);
        Assert.False(addedWebHook.Active);

        //Check lists of webhooks are update
        var webhooksAfterAdd = await TrelloClient.GetWebhooksForCurrentToken();
        Assert.Equal(currentWebhooks.Count + 1, webhooksAfterAdd.Count);
        Assert.Equal(webhookId, webhooksAfterAdd.First(x=> x.Id == webhookId).Id);
        Assert.Equal(updatedDescription, webhooksAfterAdd.First(x=> x.Id == webhookId).Description);
        Assert.Equal(callbackUrl, webhooksAfterAdd.First(x=> x.Id == webhookId).CallbackUrl);

        //Get Webhook
        var getWebhook = await TrelloClient.GetWebhookAsync(webhookId);
        Assert.Equal(getWebhook.Id, updatedWebHook.Id);
        Assert.Equal(getWebhook.Description, updatedWebHook.Description);
        Assert.Equal(getWebhook.CallbackUrl, updatedWebHook.CallbackUrl);

        //Delete Webhook
        await TrelloClient.DeleteWebhook(webhookId);
        var webhooksAfterDelete = await TrelloClient.GetWebhooksForCurrentToken();
        Assert.Equal(currentWebhooks.Count, webhooksAfterDelete.Count);
    }
}
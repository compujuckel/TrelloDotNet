﻿using TrelloDotNet.Model;

namespace TrelloDotNet.Tests.IntegrationTests;

public class ListTests : TestBaseWithNewBoard
{
    [Fact]
    public async Task ListCrud()
    {
        var listsBefore = await TrelloClient.GetListsOnBoardAsync(BoardId);

        //Add List Test
        var newListName = Guid.NewGuid().ToString();
        var addedList = await TrelloClient.AddListAsync(new List(newListName, BoardId));
        var listId = addedList.Id;
        AssertTimeIsNow(addedList.Created);
        Assert.False(addedList.Closed);
        Assert.Equal(listId, addedList.Id);
        Assert.Equal(newListName, addedList.Name);
        
        //There are now one more list
        var listsAfter = await TrelloClient.GetListsOnBoardAsync(BoardId);
        Assert.Equal(listsBefore.Count + 1, listsAfter.Count);

        //New list is there
        var newList = listsAfter.FirstOrDefault(x => x.Name == newListName);
        Assert.NotNull(newList);

        //Update List
        var updatedName = Guid.NewGuid().ToString();
        newList.Name = updatedName;
        var newListUpdated = await TrelloClient.UpdateListAsync(newList);
        var getnewListViaId = await TrelloClient.GetListAsync(listId);
        Assert.Equal(updatedName, getnewListViaId.Name);
        Assert.Equal(newListUpdated.Name, getnewListViaId.Name);

        //Archive List
        var archivedList = await TrelloClient.ArchiveListAsync(newListUpdated.Id);
        var listsAfterArchive = await TrelloClient.GetListsOnBoardAsync(BoardId);
        Assert.True(archivedList.Closed);
        Assert.Equal(listsBefore.Count, listsAfterArchive.Count);
        Assert.True(listsAfterArchive.TrueForAll(x=> x.Id != listId));
        Assert.True(listsAfterArchive.TrueForAll(x=> x.Name != updatedName));

        //Check that there are now one closed list
        var closedLists = await TrelloClient.GetListsOnBoardFilteredAsync(BoardId, ListFilter.Closed);
        Assert.Single(closedLists);
        Assert.Equal(listId, closedLists.First().Id);
        Assert.Equal(updatedName, closedLists.First().Name);

        //Reopen
        var reopenedList = await TrelloClient.ReOpenListAsync(listId);
        Assert.False(reopenedList.Closed);
        Assert.Equal(listId, reopenedList.Id);
        Assert.Equal(updatedName, reopenedList.Name);

        var listsAfterReopen = await TrelloClient.GetListsOnBoardAsync(BoardId);
        Assert.Equal(listsAfterArchive.Count + 1, listsAfterReopen.Count);

        //Test: ArchiveAllCardsInList
        //Add some cards so we can test Archive All Cards In List
        await TrelloClient.AddCardAsync(new Card(reopenedList.Id, "C1"));
        await TrelloClient.AddCardAsync(new Card(reopenedList.Id, "C2"));
        await TrelloClient.AddCardAsync(new Card(reopenedList.Id, "C3"));
        var cardsOnListAfterAdd = await TrelloClient.GetCardsInListAsync(reopenedList.Id);
        Assert.Equal(3, cardsOnListAfterAdd.Count);
        await TrelloClient.ArchiveAllCardsInList(reopenedList.Id);
        var cardsOnListAfterArchive = await TrelloClient.GetCardsInListAsync(reopenedList.Id);
        Assert.Empty(cardsOnListAfterArchive);
        
        //Test: Move All Cards In List
        //Add some cards so we can test Archive All Cards In List
        await TrelloClient.AddCardAsync(new Card(reopenedList.Id, "C1"));
        await TrelloClient.AddCardAsync(new Card(reopenedList.Id, "C2"));
        await TrelloClient.AddCardAsync(new Card(reopenedList.Id, "C3"));
        //Add new list to move cards to
        var listToMoveTo = await TrelloClient.AddListAsync(new List("List to move to", BoardId));
        await TrelloClient.MoveAllCardsInList(reopenedList.Id, listToMoveTo.Id);
        var cardsOnListAfterMove = await TrelloClient.GetCardsInListAsync(listToMoveTo.Id);
        Assert.Equal(3, cardsOnListAfterMove.Count);
    }
}
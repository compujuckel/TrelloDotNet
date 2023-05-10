﻿using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TrelloDotNet.Model;

namespace TrelloDotNet
{
    public partial class TrelloClient
    {
        /// <summary>
        /// Add a new Board
        /// </summary>
        /// <param name="board">The Board to Add</param>
        /// <param name="options">Options for the new board</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The New Board</returns>
        public async Task<Board> AddBoardAsync(Board board, AddBoardOptions options = null, CancellationToken cancellationToken = default)
        {
            var parameters = _queryParametersBuilder.GetViaQueryParameterAttributes(board).ToList();
            if (options != null)
            {
                parameters.AddRange(_queryParametersBuilder.GetViaQueryParameterAttributes(options));
            }

            return await _apiRequestController.Post<Board>($"{UrlPaths.Boards}", cancellationToken, parameters.ToArray());
        }

        /// <summary>
        /// Close (Archive) a Board
        /// </summary>
        /// <param name="boardId">The id of board that should be closed</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Closed Board</returns>
        public async Task<Board> CloseBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            return await _apiRequestController.Put<Board>($"{UrlPaths.Boards}/{boardId}", cancellationToken, new QueryParameter(@"closed", true));
        }

        /// <summary>
        /// ReOpen a Board
        /// </summary>
        /// <param name="boardId">The id of board that should be reopened</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The ReOpened Board</returns>
        public async Task<Board> ReOpenBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            return await _apiRequestController.Put<Board>($"{UrlPaths.Boards}/{boardId}", cancellationToken, new QueryParameter(@"closed", false));
        }

        /// <summary>
        /// Update a Board
        /// </summary>
        /// <param name="boardWithChanges">The board with the changes</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Updated Card</returns>
        public async Task<Board> UpdateBoardAsync(Board boardWithChanges, CancellationToken cancellationToken = default)
        {
            return await _apiRequestController.Put<Board>($"{UrlPaths.Boards}/{boardWithChanges.Id}", cancellationToken, _queryParametersBuilder.GetViaQueryParameterAttributes(boardWithChanges));
        }

        /// <summary>
        /// Delete a entire board (WARNING: THERE IS NO WAY GOING BACK!!!). Alternative use CloseBoard() for non-permanency
        /// </summary>
        /// <remarks>
        /// As this is a major thing, there is a secondary confirm needed by setting: Options.AllowDeleteOfBoards = true
        /// </remarks>
        /// <param name="boardId">The id of the Board to Delete</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        public async Task DeleteBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            if (Options.AllowDeleteOfBoards)
            {
                await _apiRequestController.Delete($"{UrlPaths.Boards}/{boardId}", cancellationToken);
            }
            else
            {
                throw new SecurityException(@"Deletion of Boards are disabled via Options.AllowDeleteOfBoards (You need to enable this as a secondary confirmation that you REALLY wish to use that option as there is no going back: https://support.atlassian.com/trello/docs/deleting-a-board/)");
            }
        }

        /// <summary>
        /// Get a Board by its Id
        /// </summary>
        /// <param name="boardId">Id of the Board (in its long or short version)</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Board</returns>
        public async Task<Board> GetBoardAsync(string boardId, CancellationToken cancellationToken = default)
        {
            return await _apiRequestController.Get<Board>($"{UrlPaths.Boards}/{boardId}", cancellationToken);
        }

        /// <summary>
        /// Get the Boards that the specified member have access to
        /// </summary>
        /// <param name="memberId">Id of the Member to find boards for</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Active Boards there is access to</returns>
        public async Task<List<Board>> GetBoardsForMemberAsync(string memberId, CancellationToken cancellationToken = default)
        {
            return await _apiRequestController.Get<List<Board>>($"{UrlPaths.Members}/{memberId}/boards", cancellationToken);
        }

        /// <summary>
        /// Get the Boards that the token provided to the TrelloClient can Access
        /// </summary>
        /// <returns>The Active Boards there is access to</returns>
        public async Task<List<Board>> GetBoardsCurrentTokenCanAccessAsync(CancellationToken cancellationToken = default)
        {
            var tokenMember = await GetTokenMemberAsync(cancellationToken);
            return await GetBoardsForMemberAsync(tokenMember.Id, cancellationToken);
        }
    }
}
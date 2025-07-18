﻿using AutoMapper;
using LoginUpLevel.DTOs;
using LoginUpLevel.Models;
using LoginUpLevel.Repositories.Interface;
using LoginUpLevel.Services.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace LoginUpLevel.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _uniOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private const string CommentCacheKey = "comment_list";
        public CommentService(IUnitOfWork uniOfWork, IMapper mapper, IMemoryCache cache)
        {
            _uniOfWork = uniOfWork;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task AddCommentAsync(CommentDTO commentDto)
        {
            try
            {
                if(!await _uniOfWork.OrderDetailRepository.HasCustomerPurchasedProductCompletedAsync(commentDto.CustomerId, commentDto.ProductId))
                {
                    throw new Exception("You can only comment on products you have purchased.");
                }
                var comment = _mapper.Map<Comment>(commentDto);
                await _uniOfWork.CommentRepository.Add(comment);
                await _uniOfWork.SaveChangesAsync();

                _cache.Remove(CommentCacheKey);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while adding the comment.", ex);
            }
        }

        public async Task DeleteCommentAsync(int id)
        {
            try
            {
                var comment = await _uniOfWork.CommentRepository.GetById(id);
                if (comment == null)
                {
                    throw new KeyNotFoundException("Comment not found.");
                }
                await _uniOfWork.CommentRepository.Delete(comment);
                await _uniOfWork.SaveChangesAsync();

                _cache.Remove(CommentCacheKey);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while deleting the comment.", ex);
            }
        }

        public async Task<IEnumerable<CommentDTO>> GetCommentsByProductIdAsync(int productId)
        {
            return await _uniOfWork.CommentRepository.GetCommentsByProductIdAsync(productId)
                .ContinueWith(task => _mapper.Map<IEnumerable<CommentDTO>>(task.Result));
        }

        public async Task<IEnumerable<CommentDTO>> GetCommentsByRatingAsync(int productId,int rating)
        {
            try
            {
                var commentsDto = await _uniOfWork.CommentRepository.GetCommentsByRatingAsyns(productId,rating)
                    .ContinueWith(task => _mapper.Map<IEnumerable<CommentDTO>>(task.Result));

                _cache.Set(CommentCacheKey, commentsDto, TimeSpan.FromMinutes(30));
                return commentsDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateCommentAsync(CommentDTO commentDto, int customerId)
        {
            try
            {
                var oldComment = await _uniOfWork.CommentRepository.GetById(commentDto.Id);
                if (oldComment.CustomerId != customerId)
                {
                    throw new KeyNotFoundException("You only update your owm comment");
                }
                commentDto.CustomerId = customerId;
                _mapper.Map(commentDto, oldComment);
                await _uniOfWork.CommentRepository.Update(oldComment);
                await _uniOfWork.SaveChangesAsync();
                _cache.Remove(CommentCacheKey);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while updating the comment.", ex);
            }
        }
    }
}
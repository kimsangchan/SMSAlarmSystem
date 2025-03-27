// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 기본 저장소 구현 클래스
// 설명: 이 클래스는 IRepository 인터페이스를 구현하며, 기본적인 CRUD 작업을 제공합니다.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        public Repository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 모든 항목 가져오기
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug($"Repository.GetAllAsync 호출: 엔티티 타입 {typeof(T).Name}");
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Repository.GetAllAsync 오류: {ex.Message}");
                return new List<T>();
            }
        }

        // 조건에 맞는 항목 가져오기
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug($"Repository.FindAsync 호출: 엔티티 타입 {typeof(T).Name}");
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Repository.FindAsync 오류: {ex.Message}");
                return new List<T>();
            }
        }

        // ID로 항목 가져오기
        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug($"Repository.GetByIdAsync 호출: 엔티티 타입 {typeof(T).Name}, ID {id}");
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Repository.GetByIdAsync 오류: {ex.Message}");
                return null;
            }
        }

        // 항목 추가
        public async Task AddAsync(T entity)
        {
            try
            {
                _logger.LogDebug($"Repository.AddAsync 호출: 엔티티 타입 {typeof(T).Name}");
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Repository.AddAsync 오류: {ex.Message}");
                throw;
            }
        }

        // 항목 업데이트
        public async Task UpdateAsync(T entity)
        {
            try
            {
                _logger.LogDebug($"Repository.UpdateAsync 호출: 엔티티 타입 {typeof(T).Name}");
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Repository.UpdateAsync 오류: {ex.Message}");
                throw;
            }
        }

        // 항목 삭제
        public async Task DeleteAsync(T entity)
        {
            try
            {
                _logger.LogDebug($"Repository.DeleteAsync 호출: 엔티티 타입 {typeof(T).Name}");
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Repository.DeleteAsync 오류: {ex.Message}");
                throw;
            }
        }
    }
}

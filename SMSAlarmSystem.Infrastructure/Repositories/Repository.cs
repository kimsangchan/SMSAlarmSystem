// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 제네릭 저장소 기본 구현 클래스
// 설명: 이 클래스는 IRepository<T> 인터페이스를 구현하며, 모든 엔티티 타입에 대한 기본적인 CRUD 작업을 제공합니다.

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
    /// <summary>
    /// 제네릭 저장소 클래스
    /// </summary>
    /// <typeparam name="T">저장소가 다루는 엔티티 타입</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        /// <summary>
        /// Repository 클래스의 생성자
        /// </summary>
        /// <param name="context">데이터베이스 컨텍스트</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">context 또는 logger가 null인 경우 발생</exception>
        public Repository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "데이터베이스 컨텍스트는 null이 될 수 없습니다.");
            _dbSet = context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");
        }

        /// <summary>
        /// 모든 엔티티를 가져옵니다.
        /// </summary>
        /// <returns>엔티티 컬렉션</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation($"모든 {typeof(T).Name} 엔티티 조회 시작");
                var entities = await _dbSet.ToListAsync();
                _logger.LogInformation($"{entities.Count}개의 {typeof(T).Name} 엔티티 조회 완료");
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 전체 조회 중 오류 발생");
                throw; // 상위 계층에서 예외 처리할 수 있도록 예외를 다시 던집니다.
            }
        }

        /// <summary>
        /// 주어진 조건에 맞는 엔티티들을 찾습니다.
        /// </summary>
        /// <param name="predicate">검색 조건</param>
        /// <returns>조건에 맞는 엔티티 컬렉션</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "검색 조건은 null이 될 수 없습니다.");
            }

            try
            {
                _logger.LogInformation($"{typeof(T).Name} 엔티티 조건 검색 시작");
                var entities = await _dbSet.Where(predicate).ToListAsync();
                _logger.LogInformation($"{entities.Count}개의 {typeof(T).Name} 엔티티 조건 검색 완료");
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 조건 검색 중 오류 발생");
                throw;
            }
        }

        /// <summary>
        /// ID로 특정 엔티티를 가져옵니다.
        /// </summary>
        /// <param name="id">찾고자 하는 엔티티의 ID</param>
        /// <returns>찾은 엔티티 또는 null</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"{typeof(T).Name} 엔티티 ID {id} 조회 시작");
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning($"{typeof(T).Name} 엔티티 ID {id}를 찾을 수 없습니다.");
                }
                else
                {
                    _logger.LogInformation($"{typeof(T).Name} 엔티티 ID {id} 조회 완료");
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 ID {id} 조회 중 오류 발생");
                throw;
            }
        }

        /// <summary>
        /// 새 엔티티를 추가합니다.
        /// </summary>
        /// <param name="entity">추가할 엔티티</param>
        public async Task AddAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "추가할 엔티티는 null이 될 수 없습니다.");
            }

            try
            {
                _logger.LogInformation($"새 {typeof(T).Name} 엔티티 추가 시작");
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"새 {typeof(T).Name} 엔티티 추가 완료");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 추가 중 데이터베이스 오류 발생");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 추가 중 오류 발생");
                throw;
            }
        }

        /// <summary>
        /// 기존 엔티티를 업데이트합니다.
        /// </summary>
        /// <param name="entity">업데이트할 엔티티</param>
        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "업데이트할 엔티티는 null이 될 수 없습니다.");
            }

            try
            {
                _logger.LogInformation($"{typeof(T).Name} 엔티티 업데이트 시작");
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{typeof(T).Name} 엔티티 업데이트 완료");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 업데이트 중 데이터베이스 오류 발생");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 업데이트 중 오류 발생");
                throw;
            }
        }

        /// <summary>
        /// 엔티티를 삭제합니다.
        /// </summary>
        /// <param name="entity">삭제할 엔티티</param>
        public async Task DeleteAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "삭제할 엔티티는 null이 될 수 없습니다.");
            }

            try
            {
                _logger.LogInformation($"{typeof(T).Name} 엔티티 삭제 시작");
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{typeof(T).Name} 엔티티 삭제 완료");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 삭제 중 데이터베이스 오류 발생");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{typeof(T).Name} 엔티티 삭제 중 오류 발생");
                throw;
            }
        }
    }
}

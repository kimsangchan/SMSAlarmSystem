// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 알람 포인트 데이터를 관리하는 리포지토리 클래스
// 설명: 이 클래스는 데이터베이스와 상호작용하여 알람 포인트 데이터를 CRUD 방식으로 관리합니다.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    /// <summary>
    /// 알람 포인트 데이터를 관리하는 리포지토리 클래스
    /// </summary>
    public class AlarmPointRepository : Repository<AlarmPoint>, IAlarmPointRepository
    {
        // 데이터베이스 컨텍스트 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ApplicationDbContext _context;

        // 로거 인스턴스 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ILogger<AlarmPointRepository> _logger;

        /// <summary>
        /// AlarmPointRepository 생성자
        /// </summary>
        /// <param name="context">데이터베이스 컨텍스트</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public AlarmPointRepository(ApplicationDbContext context, ILogger<AlarmPointRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "데이터베이스 컨텍스트는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");
        }
        /// <summary>
        /// 모든 알람 포인트를 조회합니다.
        /// 기본 클래스의 메서드를 숨기고 MessageGroup을 포함하여 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        public new async Task<IEnumerable<AlarmPoint>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("모든 알람 포인트 조회 시작");

                var alarmPoints = await _context.AlarmPoints
                    .Include(a => a.MessageGroup)
                    .ToListAsync();

                _logger.LogInformation("모든 알람 포인트 조회 완료: {Count}개 항목", alarmPoints.Count);
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 알람 포인트 조회 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }
        /// <summary>
        /// ID로 알람 포인트를 조회합니다.
        /// 기본 클래스의 메서드를 숨기고 MessageGroup을 포함하여 조회합니다.
        /// </summary>
        /// <param name="id">알람 포인트 ID</param>
        /// <returns>알람 포인트 객체 또는 null</returns>
        public new async Task<AlarmPoint?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("ID {Id}로 알람 포인트 조회 시작", id);

                var alarmPoint = await _context.AlarmPoints
                    .Include(a => a.MessageGroup)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (alarmPoint == null)
                {
                    _logger.LogInformation("ID {Id}인 알람 포인트를 찾을 수 없습니다.", id);
                }
                else
                {
                    _logger.LogInformation("ID {Id}인 알람 포인트를 찾았습니다: {Name}", id, alarmPoint.Name);
                }

                return alarmPoint;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID {Id}인 알람 포인트 조회 중 오류 발생: {Message}", id, ex.Message);
                throw;
            }
        }
        /// <remarks>
        /// 작성자: Sangchan, Kim
        /// 작성일: 2025-04-03
        /// 기능: 데이터베이스에서 이름으로 알람 포인트를 조회
        /// 설명: 주어진 이름과 일치하는 알람 포인트를 데이터베이스에서 찾아 반환합니다.
        ///       이름이 null이거나 빈 문자열인 경우, 또는 일치하는 알람 포인트가 없는 경우 null을 반환합니다.
        /// </remarks>
        public async Task<AlarmPoint?> GetByNameAsync(string name)
        {
            try
            {
                // null 또는 빈 문자열 체크
                if (string.IsNullOrEmpty(name))
                {
                    _logger.LogWarning("알람 포인트 이름이 null 또는 빈 문자열입니다.");
                    return null;
                }

                // 데이터베이스에서 이름으로 알람 포인트 조회
                var alarmPoint = await _context.AlarmPoints
                    .FirstOrDefaultAsync(a => a.Name == name);

                if (alarmPoint == null)
                {
                    _logger.LogInformation("이름이 '{Name}'인 알람 포인트를 찾을 수 없습니다.", name);
                }
                else
                {
                    _logger.LogInformation("이름이 '{Name}'인 알람 포인트를 찾았습니다. ID: {Id}", name, alarmPoint.Id);
                }

                return alarmPoint;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "이름이 '{Name}'인 알람 포인트 조회 중 오류 발생: {Message}", name, ex.Message);
                throw;
            }
        }

        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 활성화된 알람 포인트 조회
        // 설명: IsActive가 true인 알람 포인트만 조회합니다.

        /// <summary>
        /// 활성화된 알람 포인트만 조회합니다.
        /// </summary>
        /// <returns>활성화된 알람 포인트 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 알람 포인트 조회 시작");

                // 데이터베이스에서 활성화된 알람 포인트만 조회
                var alarmPoints = await _context.AlarmPoints
                    .Where(a => a.IsActive)
                    .Include(a => a.MessageGroup)
                    .ToListAsync();

                _logger.LogInformation("활성화된 알람 포인트 조회 완료: {Count}개 항목", alarmPoints.Count);
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 알람 포인트 조회 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }


        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 그룹 ID로 알람 포인트 조회
        // 설명: 특정 메시지 그룹에 연결된 알람 포인트를 조회합니다.

        /// <summary>
        /// 특정 그룹에 연결된 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 연결된 알람 포인트 목록</returns>
        /// <exception cref="ArgumentException">groupId가 0 이하인 경우</exception>
        public async Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId)
        {
            try
            {
                // 매개변수 유효성 검사
                if (groupId <= 0)
                {
                    throw new ArgumentException("그룹 ID는 0보다 커야 합니다.", nameof(groupId));
                }

                _logger.LogInformation("그룹 ID {GroupId}로 알람 포인트 조회 시작", groupId);

                // 데이터베이스에서 그룹 ID로 알람 포인트 조회
                var alarmPoints = await _context.AlarmPoints
                    .Where(a => a.MessageGroupId == groupId)
                    .Include(a => a.MessageGroup)
                    .ToListAsync();

                _logger.LogInformation("그룹 ID {GroupId}에 대해 {Count}개의 알람 포인트를 조회했습니다.",
                    groupId, alarmPoints.Count);

                return alarmPoints;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                _logger.LogError(ex, "그룹 ID {GroupId}로 알람 포인트 조회 중 오류 발생: {Message}",
                    groupId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 알람 포인트를 추가합니다.
        /// 기본 클래스의 메서드를 숨기고 중복 이름 체크 등의 추가 로직을 수행합니다.
        /// </summary>
        /// <param name="alarmPoint">추가할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        /// <exception cref="ArgumentNullException">alarmPoint가 null인 경우</exception>
        public new async Task AddAsync(AlarmPoint alarmPoint)
        {
            try
            {
                // null 체크
                if (alarmPoint == null)
                {
                    throw new ArgumentNullException(nameof(alarmPoint), "알람 포인트는 null이 될 수 없습니다.");
                }

                // 중복 이름 체크
                var existingAlarmPoint = await _context.AlarmPoints
                    .FirstOrDefaultAsync(a => a.Name == alarmPoint.Name);

                if (existingAlarmPoint != null)
                {
                    _logger.LogWarning("이름이 '{Name}'인 알람 포인트가 이미 존재합니다.", alarmPoint.Name);
                    throw new InvalidOperationException($"이름이 '{alarmPoint.Name}'인 알람 포인트가 이미 존재합니다.");
                }

                // 알람 포인트 추가
                await _context.AlarmPoints.AddAsync(alarmPoint);
                await _context.SaveChangesAsync();

                _logger.LogInformation("새 알람 포인트가 추가되었습니다. ID: {Id}, 이름: {Name}", alarmPoint.Id, alarmPoint.Name);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "알람 포인트 추가 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 알람 포인트를 업데이트합니다.
        /// 기본 클래스의 메서드를 숨기고 중복 이름 체크 등의 추가 로직을 수행합니다.
        /// </summary>
        /// <param name="alarmPoint">업데이트할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        /// <exception cref="ArgumentNullException">alarmPoint가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">알람 포인트를 찾을 수 없는 경우</exception>
        public new async Task UpdateAsync(AlarmPoint alarmPoint)
        {
            try
            {
                // null 체크
                if (alarmPoint == null)
                {
                    throw new ArgumentNullException(nameof(alarmPoint), "알람 포인트는 null이 될 수 없습니다.");
                }

                // 존재 여부 확인
                var existingAlarmPoint = await _context.AlarmPoints.FindAsync(alarmPoint.Id);
                if (existingAlarmPoint == null)
                {
                    _logger.LogWarning("ID {Id}인 알람 포인트를 찾을 수 없습니다.", alarmPoint.Id);
                    throw new InvalidOperationException($"ID {alarmPoint.Id}인 알람 포인트를 찾을 수 없습니다.");
                }

                // 중복 이름 체크 (다른 알람 포인트와 이름 중복 방지)
                var duplicateAlarmPoint = await _context.AlarmPoints
                    .FirstOrDefaultAsync(a => a.Name == alarmPoint.Name && a.Id != alarmPoint.Id);

                if (duplicateAlarmPoint != null)
                {
                    _logger.LogWarning("이름이 '{Name}'인 다른 알람 포인트가 이미 존재합니다.", alarmPoint.Name);
                    throw new InvalidOperationException($"이름이 '{alarmPoint.Name}'인 다른 알람 포인트가 이미 존재합니다.");
                }

                // 속성 업데이트
                existingAlarmPoint.Name = alarmPoint.Name;
                existingAlarmPoint.Description = alarmPoint.Description;
                existingAlarmPoint.Condition = alarmPoint.Condition;
                existingAlarmPoint.MessageGroupId = alarmPoint.MessageGroupId;
                existingAlarmPoint.IsActive = alarmPoint.IsActive;

                // 변경 사항 저장
                _context.AlarmPoints.Update(existingAlarmPoint);
                await _context.SaveChangesAsync();

                _logger.LogInformation("알람 포인트가 업데이트되었습니다. ID: {Id}, 이름: {Name}", alarmPoint.Id, alarmPoint.Name);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "알람 포인트 업데이트 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }


        /// <summary>
        /// 알람 포인트를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 알람 포인트 ID</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        /// <exception cref="InvalidOperationException">알람 포인트를 찾을 수 없는 경우</exception>
        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("ID {Id}인 알람 포인트 삭제 시작", id);

                // 알람 포인트 조회
                var alarmPoint = await _context.AlarmPoints.FindAsync(id);
                if (alarmPoint == null)
                {
                    _logger.LogWarning("ID {Id}인 알람 포인트를 찾을 수 없습니다.", id);
                    throw new InvalidOperationException($"ID {id}인 알람 포인트를 찾을 수 없습니다.");
                }

                // 알람 포인트 삭제
                _context.AlarmPoints.Remove(alarmPoint);
                await _context.SaveChangesAsync();

                _logger.LogInformation("ID {Id}인 알람 포인트가 삭제되었습니다.", id);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, "ID {Id}인 알람 포인트 삭제 중 오류 발생: {Message}", id, ex.Message);
                throw;
            }
        }

        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 알람 포인트 리포지토리 확장 메서드
        // 설명: Entity Framework 캐시 문제 해결 및 중복 체크 없는 추가 기능 제공

        /// <summary>
        /// Entity Framework 컨텍스트 캐시를 초기화합니다.
        /// </summary>
        public void ClearContext()
        {
            _context.ChangeTracker.Clear();
        }

        /// <summary>
        /// 추적 없이 모든 알람 포인트를 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        public async Task<IEnumerable<AlarmPoint>> GetAllAsNoTrackingAsync()
        {
            try
            {
                _logger.LogInformation("추적 없이 모든 알람 포인트 조회 시작");

                var alarmPoints = await _context.AlarmPoints
                    .AsNoTracking()  // 추적 비활성화
                    .ToListAsync();

                _logger.LogInformation("추적 없이 모든 알람 포인트 조회 완료: {Count}개 항목", alarmPoints.Count);
                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "추적 없이 모든 알람 포인트 조회 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 중복 체크 없이 알람 포인트를 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">추가할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        public async Task AddWithoutDuplicateCheckAsync(AlarmPoint alarmPoint)
        {
            try
            {
                // null 체크
                if (alarmPoint == null)
                {
                    throw new ArgumentNullException(nameof(alarmPoint), "알람 포인트는 null이 될 수 없습니다.");
                }

                // 알람 포인트 추가 (중복 체크 없음)
                await _context.AlarmPoints.AddAsync(alarmPoint);
                await _context.SaveChangesAsync();

                _logger.LogInformation("새 알람 포인트가 추가되었습니다. 이름: {Name}", alarmPoint.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 추가 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }
        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 알람 포인트 업데이트 또는 추가
        // 설명: 기존 알람 포인트가 있으면 업데이트하고, 없으면 새로 추가합니다.

        /// <summary>
        /// 알람 포인트를 업데이트하거나 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">업데이트 또는 추가할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        /// <exception cref="ArgumentNullException">alarmPoint가 null인 경우</exception>
        public async Task UpdateOrAddAsync(AlarmPoint alarmPoint)
        {
            if (alarmPoint == null)
            {
                throw new ArgumentNullException(nameof(alarmPoint), "알람 포인트는 null이 될 수 없습니다.");
            }

            try
            {
                var existingAlarmPoint = await _context.AlarmPoints
                    .FirstOrDefaultAsync(a => a.Name == alarmPoint.Name);

                if (existingAlarmPoint != null)
                {
                    // 기존 알람 포인트 업데이트
                    existingAlarmPoint.Description = alarmPoint.Description;
                    existingAlarmPoint.Condition = alarmPoint.Condition;
                    existingAlarmPoint.IsActive = alarmPoint.IsActive;
                    existingAlarmPoint.MessageGroupId = alarmPoint.MessageGroupId;
                    existingAlarmPoint.UpdatedAt = DateTime.Now;

                    _context.AlarmPoints.Update(existingAlarmPoint);
                    _logger.LogInformation("알람 포인트가 업데이트되었습니다. 이름: {Name}", existingAlarmPoint.Name);
                }
                else
                {
                    // 새 알람 포인트 추가
                    await _context.AlarmPoints.AddAsync(alarmPoint);
                    _logger.LogInformation("새 알람 포인트가 추가되었습니다. 이름: {Name}", alarmPoint.Name);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "알람 포인트 업데이트 또는 추가 중 데이터베이스 오류 발생: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 업데이트 또는 추가 중 오류 발생: {Message}", ex.Message);
                throw;
            }
        }


    }
}

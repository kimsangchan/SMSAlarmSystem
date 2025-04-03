// 작성자: Sangchan, Kim
// 작성일: 2025-04-03
// 기능: 알람 포인트 리포지토리 인터페이스
// 설명: 알람 포인트 데이터에 접근하기 위한 메서드를 정의합니다.

using System.Collections.Generic;
using System.Threading.Tasks;
using SMSAlarmSystem.Core.Models;

namespace SMSAlarmSystem.Core.Interfaces
{
    /// <summary>
    /// 알람 포인트 데이터에 접근하기 위한 인터페이스
    /// </summary>
    public interface IAlarmPointRepository
    {
        /// <summary>
        /// 모든 알람 포인트를 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetAllAsync();

        /// <summary>
        /// ID로 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="id">알람 포인트 ID</param>
        /// <returns>알람 포인트 객체 또는 null</returns>
        Task<AlarmPoint?> GetByIdAsync(int id);

        /// <summary>
        /// 이름으로 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="name">알람 포인트 이름</param>
        /// <returns>알람 포인트 객체 또는 null</returns>
        Task<AlarmPoint?> GetByNameAsync(string name);

        /// <summary>
        /// 알람 포인트를 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">추가할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        Task AddAsync(AlarmPoint alarmPoint);

        /// <summary>
        /// 알람 포인트를 업데이트합니다.
        /// </summary>
        /// <param name="alarmPoint">업데이트할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        Task UpdateAsync(AlarmPoint alarmPoint);

        /// <summary>
        /// 알람 포인트를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 알람 포인트 ID</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// 특정 그룹에 연결된 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 연결된 알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetAlarmPointsByGroupIdAsync(int groupId);

        /// <summary>
        /// 활성화된 알람 포인트만 조회합니다.
        /// </summary>
        /// <returns>활성화된 알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync();
        /// <summary>
        /// Entity Framework 컨텍스트 캐시를 초기화합니다.
        /// </summary>
        void ClearContext();

        /// <summary>
        /// 추적 없이 모든 알람 포인트를 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetAllAsNoTrackingAsync();

        /// <summary>
        /// 중복 체크 없이 알람 포인트를 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">추가할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        Task AddWithoutDuplicateCheckAsync(AlarmPoint alarmPoint);
        /// <summary>
        /// 알람 포인트를 업데이트하거나 추가합니다.
        /// </summary>
        /// <param name="alarmPoint">업데이트 또는 추가할 알람 포인트</param>
        /// <returns>작업 완료를 나타내는 Task</returns>
        /// <exception cref="ArgumentNullException">alarmPoint가 null인 경우</exception>
        Task UpdateOrAddAsync(AlarmPoint alarmPoint);
    }
}

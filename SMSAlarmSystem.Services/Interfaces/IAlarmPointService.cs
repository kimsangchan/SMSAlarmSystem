// 작성자: Sangchan, Kim
// 작성일: 2025-04-02
// 기능: 알람 포인트 서비스
// 설명: 외부 데이터베이스의 알람 포인트를 내부 시스템에서 사용할 수 있도록 변환하는 서비스입니다.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace SMSAlarmSystem.Services
{
    /// <summary>
    /// 알람 포인트 서비스 인터페이스
    /// </summary>
    public interface IAlarmPointService
    {
        /// <summary>
        /// 외부 데이터베이스에서 중복이 제거된 알람 포인트 목록을 조회합니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetExternalAlarmPointsAsync();

        /// <summary>
        /// 특정 시스템 코드와 부모 ID에 해당하는 알람 포인트 목록을 조회합니다.
        /// </summary>
        /// <param name="systemCode">시스템 코드</param>
        /// <param name="parentId">부모 ID</param>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<AlarmPoint>> GetAlarmPointsBySystemCodeAndParentIdAsync(int systemCode, int parentId);

        /// <summary>
        /// 외부 데이터베이스에서 알람 포인트를 동기화합니다.
        /// </summary>
        /// <returns>동기화된 알람 포인트 수</returns>
        Task<int> SynchronizeAlarmPointsAsync();

    }
}

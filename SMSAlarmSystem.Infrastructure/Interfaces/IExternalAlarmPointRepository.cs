// 작성자: Sangchan, Kim
// 작성일: 2025-04-02
// 기능: 외부 데이터베이스 알람 포인트 인터페이스 및 리포지토리 구현
// 설명: IBSInfo 데이터베이스의 P_OBJ_CODE와 P_OBJECT 테이블을 조인하여 중복 없는 알람 포인트 데이터를 조회합니다.
// ALARM_LV = 1 조건을 추가하여 특정 알람 레벨의 포인트만 조회합니다.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMSAlarmSystem.Core.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    /// <summary>
    /// 외부 데이터베이스의 알람 포인트 데이터에 접근하기 위한 인터페이스
    /// </summary>
    public interface IExternalAlarmPointRepository
    {
        /// <summary>
        /// 중복이 제거된 알람 포인트 목록을 조회합니다. ALARM_LV = 1 조건이 적용됩니다.
        /// </summary>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<ExternalAlarmPointDto>> GetDistinctAlarmPointsAsync();

        /// <summary>
        /// 특정 시스템 코드와 부모 ID에 해당하는 알람 포인트 목록을 조회합니다. ALARM_LV = 1 조건이 적용됩니다.
        /// </summary>
        /// <param name="systemCode">시스템 코드</param>
        /// <param name="parentId">부모 ID</param>
        /// <returns>알람 포인트 목록</returns>
        Task<IEnumerable<ExternalAlarmPointDto>> GetAlarmPointsBySystemCodeAndParentIdAsync(int systemCode, int parentId);

        /// <summary>
        /// 지정된 ID로 알람 포인트를 조회합니다.
        /// </summary>
        /// <param name="objectSeq">객체 시퀀스</param>
        /// <param name="systemId">시스템 ID</param>
        /// <param name="deviceId">장치 ID</param>
        /// <returns>알람 포인트 또는 null</returns>
        Task<ExternalAlarmPointDto> GetAlarmPointByIdsAsync(long? objectSeq, int? systemId, int? deviceId);

    }

    // 작성자: Sangchan, Kim
    // 작성일: 2025-04-03
    // 기능: 외부 알람 포인트 데이터 전송 객체 (DTO)
    // 설명: 외부 데이터베이스의 알람 포인트 정보를 담는 클래스입니다.

    public class ExternalAlarmPointDto
    {
        public long ObjectSeq { get; set; }
        public long ObjectId { get; set; }
        public int ServerId { get; set; }
        public int SystemId { get; set; }
        public int DeviceId { get; set; }
        public string ObjName { get; set; } = string.Empty; // 초기화하여 null 허용하지 않음
        public string ObjDesc { get; set; } = string.Empty; // 초기화하여 null 허용하지 않음
        public int AlarmLv { get; set; }

        public int ObjAbove { get; set; }
        public int ObjBelow { get; set; }
        public string SystemName { get; set; } = string.Empty; // 초기화하여 null 허용하지 않음

       
    // 생성자에서 문자열 속성들을 빈 문자열로 초기화
    public ExternalAlarmPointDto()
        {
            ObjName = string.Empty;
            ObjDesc = string.Empty;
            SystemName = string.Empty;
        }
    }
}
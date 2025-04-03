// 작성자: Sangchan, Kim
// 작성일: 2025-04-02
// 기능: 외부 데이터베이스 알람 포인트 리포지토리 구현
// 설명: IBSInfo 데이터베이스의 P_OBJ_CODE와 P_OBJECT 테이블을 조인하여 중복 없는 알람 포인트 데이터를 조회합니다.
//       ALARM_LV = 1 조건을 추가하여 특정 알람 레벨의 포인트만 조회합니다.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Infrastructure.Data;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    /// <summary>
    /// 외부 데이터베이스의 알람 포인트 데이터에 접근하는 리포지토리 구현
    /// </summary>
    public class ExternalAlarmPointRepository : IExternalAlarmPointRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ExternalAlarmPointRepository> _logger;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="dbSettings">데이터베이스 설정</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우</exception>
        public ExternalAlarmPointRepository(
            IOptions<ExternalDbSettings> dbSettings,
            ILogger<ExternalAlarmPointRepository> logger)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            _connectionString = dbSettings.Value.ConnectionString ?? throw new ArgumentNullException(nameof(dbSettings.Value.ConnectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 작성자: Sangchan, Kim
        // 작성일: 2025-04-03
        // 기능: 외부 데이터베이스에서 알람 포인트 데이터 읽기
        // 설명: SQL 쿼리 결과를 ExternalAlarmPointDto 객체로 변환하여 리스트에 추가합니다.
        //       모든 문자열 필드에 대해 null 체크 및 빈 문자열 할당을 수행합니다.
        public async Task<IEnumerable<ExternalAlarmPointDto>> GetDistinctAlarmPointsAsync()
        {
            var alarmPoints = new List<ExternalAlarmPointDto>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger.LogInformation("외부 데이터베이스 연결 성공");

                    string query = @"
                    WITH RankedData AS (
                      SELECT 
                        p.OBJECT_SEQ, p.OBJECT_ID, p.SERVER_ID, p.SYSTEM_ID, p.DEVICE_ID, p.OBJ_ABOVE, p.OBJ_BELOW,
                        p.OBJ_NAME, p.OBJ_DESC, p.ALARM_LV, c.CODE_NAME AS SystemName,
                        ROW_NUMBER() OVER (PARTITION BY p.OBJECT_SEQ, p.OBJECT_ID, p.SERVER_ID, p.SYSTEM_ID, p.DEVICE_ID, p.OBJ_ABOVE, p.OBJ_BELOW ORDER BY c.CODE_NAME) AS RowNum
                      FROM P_OBJECT p
                      JOIN P_OBJ_CODE c ON p.SYSTEM_ID = c.SYSTEM_CODE
                      WHERE c.CODE_NAME IS NOT NULL AND p.ALARM_LV = 1
                    )
                    SELECT OBJECT_SEQ, OBJECT_ID, SERVER_ID, SYSTEM_ID, DEVICE_ID, OBJ_ABOVE, OBJ_BELOW, OBJ_NAME, OBJ_DESC, ALARM_LV, SystemName
                    FROM RankedData
                    WHERE RowNum = 1
                    ORDER BY OBJECT_SEQ";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // 데이터베이스 결과를 DTO 객체로 변환
                            alarmPoints.Add(new ExternalAlarmPointDto
                            {
                                // 숫자 필드는 DBNull 체크 후 기본값 0 할당
                                ObjectSeq = reader["OBJECT_SEQ"] != DBNull.Value ? Convert.ToInt64(reader["OBJECT_SEQ"]) : 0,
                                ObjectId = reader["OBJECT_ID"] != DBNull.Value ? Convert.ToInt64(reader["OBJECT_ID"]) : 0,
                                ServerId = reader["SERVER_ID"] != DBNull.Value ? Convert.ToInt32(reader["SERVER_ID"]) : 0,
                                SystemId = reader["SYSTEM_ID"] != DBNull.Value ? Convert.ToInt32(reader["SYSTEM_ID"]) : 0,
                                DeviceId = reader["DEVICE_ID"] != DBNull.Value ? Convert.ToInt32(reader["DEVICE_ID"]) : 0,

                                // 문자열 필드는 DBNull 체크 후 ToString() 결과가 null일 경우 빈 문자열 할당
                                ObjName = reader["OBJ_NAME"] != DBNull.Value ? reader["OBJ_NAME"].ToString() ?? string.Empty : string.Empty,
                                ObjDesc = reader["OBJ_DESC"] != DBNull.Value ? reader["OBJ_DESC"].ToString() ?? string.Empty : string.Empty,

                                // 숫자 필드는 DBNull 체크 후 기본값 0 할당
                                AlarmLv = reader["ALARM_LV"] != DBNull.Value ? Convert.ToInt32(reader["ALARM_LV"]) : 0,
                                ObjAbove = reader["OBJ_ABOVE"] != DBNull.Value ? Convert.ToInt32(reader["OBJ_ABOVE"]) : 0,
                                ObjBelow = reader["OBJ_BELOW"] != DBNull.Value ? Convert.ToInt32(reader["OBJ_BELOW"]) : 0,
                                // 문자열 필드는 DBNull 체크 후 ToString() 결과가 null일 경우 빈 문자열 할당
                                SystemName = reader["SystemName"] != DBNull.Value ? reader["SystemName"].ToString() ?? string.Empty : string.Empty
                            });
                        }
                    }
                }

                _logger.LogInformation("외부 데이터베이스에서 {Count}개의 알람 포인트(ALARM_LV = 1)를 조회했습니다.", alarmPoints.Count);
                return alarmPoints;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "외부 데이터베이스 연결 중 SQL 오류 발생: {Message}, 오류 코드: {ErrorCode}", ex.Message, ex.Number);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 조회 중 예기치 않은 오류 발생: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 특정 시스템 코드와 부모 ID에 해당하는 알람 포인트 목록을 조회합니다. ALARM_LV = 1 조건이 추가되었습니다.
        /// </summary>
        /// <param name="systemCode">시스템 코드</param>
        /// <param name="parentId">부모 ID</param>
        /// <returns>알람 포인트 목록</returns>
        public async Task<IEnumerable<ExternalAlarmPointDto>> GetAlarmPointsBySystemCodeAndParentIdAsync(int systemCode, int parentId)
        {
            var alarmPoints = new List<ExternalAlarmPointDto>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger.LogInformation("외부 데이터베이스 연결 성공");

                    string query = @"
                    WITH RankedData AS (
                      SELECT 
                        p.OBJECT_SEQ, p.OBJECT_ID, p.SERVER_ID, p.SYSTEM_ID, p.DEVICE_ID, p.OBJ_ABOVE, p.OBJ_BELOW,
                        p.OBJ_NAME, p.OBJ_DESC, p.ALARM_LV, c.CODE_NAME AS SystemName,
                        ROW_NUMBER() OVER (PARTITION BY p.OBJECT_SEQ, p.OBJECT_ID, p.SERVER_ID, p.SYSTEM_ID, p.DEVICE_ID, p.OBJ_ABOVE, p.OBJ_BELOW ORDER BY c.CODE_NAME) AS RowNum
                      FROM P_OBJECT p
                      JOIN P_OBJ_CODE c ON p.SYSTEM_ID = c.SYSTEM_CODE
                      WHERE c.CODE_NAME IS NOT NULL AND p.ALARM_LV = 1
                        AND c.CODENO = @SystemCode AND c.PARENT_ID = @ParentId
                    )
                    SELECT OBJECT_SEQ, OBJECT_ID, SERVER_ID, SYSTEM_ID, DEVICE_ID, OBJ_ABOVE, OBJ_BELOW,OBJ_NAME, OBJ_DESC, ALARM_LV, SystemName
                    FROM RankedData
                    WHERE RowNum = 1
                    ORDER BY OBJECT_SEQ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@SystemCode", SqlDbType.Int).Value = systemCode;
                        command.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // 데이터베이스 결과를 DTO 객체로 변환
                                alarmPoints.Add(new ExternalAlarmPointDto
                                {
                                    // 숫자 필드는 DBNull 체크 후 기본값 0 할당
                                    ObjectSeq = reader["OBJECT_SEQ"] != DBNull.Value ? Convert.ToInt64(reader["OBJECT_SEQ"]) : 0,
                                    ObjectId = reader["OBJECT_ID"] != DBNull.Value ? Convert.ToInt64(reader["OBJECT_ID"]) : 0,
                                    ServerId = reader["SERVER_ID"] != DBNull.Value ? Convert.ToInt32(reader["SERVER_ID"]) : 0,
                                    SystemId = reader["SYSTEM_ID"] != DBNull.Value ? Convert.ToInt32(reader["SYSTEM_ID"]) : 0,
                                    DeviceId = reader["DEVICE_ID"] != DBNull.Value ? Convert.ToInt32(reader["DEVICE_ID"]) : 0,

                                    // 문자열 필드는 DBNull 체크 후 ToString() 결과가 null일 경우 빈 문자열 할당
                                    ObjName = reader["OBJ_NAME"] != DBNull.Value ? reader["OBJ_NAME"].ToString() ?? string.Empty : string.Empty,
                                    ObjDesc = reader["OBJ_DESC"] != DBNull.Value ? reader["OBJ_DESC"].ToString() ?? string.Empty : string.Empty,

                                    // 숫자 필드는 DBNull 체크 후 기본값 0 할당
                                    AlarmLv = reader["ALARM_LV"] != DBNull.Value ? Convert.ToInt32(reader["ALARM_LV"]) : 0,
                                    ObjAbove = reader["OBJ_ABOVE"] != DBNull.Value ? Convert.ToInt32(reader["OBJ_ABOVE"]) : 0,
                                    ObjBelow = reader["OBJ_BELOW"] != DBNull.Value ? Convert.ToInt32(reader["OBJ_BELOW"]) : 0,
                                    // 문자열 필드는 DBNull 체크 후 ToString() 결과가 null일 경우 빈 문자열 할당
                                    SystemName = reader["SystemName"] != DBNull.Value ? reader["SystemName"].ToString() ?? string.Empty : string.Empty
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("시스템 코드 {SystemCode}, 부모 ID {ParentId}에 대해 {Count}개의 알람 포인트(ALARM_LV = 1)를 조회했습니다.",
                    systemCode, parentId, alarmPoints.Count);
                return alarmPoints;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "외부 데이터베이스 연결 중 SQL 오류 발생: {Message}, 오류 코드: {ErrorCode}", ex.Message, ex.Number);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 조회 중 예기치 않은 오류 발생: {Message}", ex.Message);
                throw;
            }
        }
    }
}
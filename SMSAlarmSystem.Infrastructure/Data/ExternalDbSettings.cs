// 작성자: Sangchan, Kim
// 작성일: 2025-04-03
// 기능: 외부 데이터베이스 연결 설정
// 설명: IBSInfo 데이터베이스 연결에 필요한 설정 정보를 담는 클래스입니다.

using System;

namespace SMSAlarmSystem.Infrastructure.Data
{
    /// <summary>
    /// 외부 데이터베이스 연결 설정 클래스
    /// </summary>
    public class ExternalDbSettings
    {
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public ExternalDbSettings()
        {
            // 기본값으로 초기화
            ConnectionString = string.Empty;
            ConnectionTimeout = 30;
            RetryCount = 3;
        }

        /// <summary>
        /// 매개변수가 있는 생성자
        /// </summary>
        /// <param name="connectionString">데이터베이스 연결 문자열</param>
        /// <param name="connectionTimeout">연결 타임아웃(초)</param>
        /// <param name="retryCount">연결 재시도 횟수</param>
        public ExternalDbSettings(string connectionString, int connectionTimeout = 30, int retryCount = 3)
        {
            ConnectionString = connectionString ?? string.Empty;
            ConnectionTimeout = connectionTimeout > 0 ? connectionTimeout : 30;
            RetryCount = retryCount > 0 ? retryCount : 3;
        }

        /// <summary>
        /// 데이터베이스 연결 문자열
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 연결 타임아웃(초)
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// 연결 재시도 횟수
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 연결 문자열이 유효한지 확인합니다.
        /// </summary>
        /// <returns>유효성 여부</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ConnectionString);
        }

        /// <summary>
        /// 연결 문자열을 가져오거나 기본값을 반환합니다.
        /// </summary>
        /// <param name="defaultConnection">기본 연결 문자열</param>
        /// <returns>연결 문자열 또는 기본값</returns>
        public string GetConnectionStringOrDefault(string defaultConnection)
        {
            if (string.IsNullOrWhiteSpace(defaultConnection))
            {
                throw new ArgumentException("기본 연결 문자열은 null이나 빈 문자열이 될 수 없습니다.", nameof(defaultConnection));
            }

            return string.IsNullOrWhiteSpace(ConnectionString)
                ? defaultConnection
                : ConnectionString;
        }

        /// <summary>
        /// 현재 설정의 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>설정 정보 문자열</returns>
        public override string ToString()
        {
            // 보안을 위해 연결 문자열은 마스킹 처리
            string maskedConnectionString = !string.IsNullOrEmpty(ConnectionString)
                ? "********"
                : "<없음>";

            return $"ExternalDbSettings [ConnectionTimeout={ConnectionTimeout}초, RetryCount={RetryCount}회, ConnectionString={maskedConnectionString}]";
        }
    }
}

// 작성자: Sangchan, Kim
// 작성일: 2025-04-01
// 기능: 메인 대시보드 컨트롤러
// 설명: SMS 알람 시스템의 메인 대시보드 화면을 제공하는 컨트롤러입니다.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    /// <summary>
    /// 메인 대시보드 화면을 제공하는 컨트롤러
    /// 시스템 개요, 통계 정보 및 최근 활동을 표시합니다.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IAlarmService _alarmService;
        private readonly IMessageGroupService _messageGroupService;
        private readonly IMemberService _memberService;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// HomeController 생성자
        /// </summary>
        /// <param name="messageService">메시지 서비스 인스턴스</param>
        /// <param name="alarmService">알람 서비스 인스턴스</param>
        /// <param name="messageGroupService">메시지 그룹 서비스 인스턴스</param>
        /// <param name="memberService">회원 서비스 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public HomeController(
            IMessageService messageService,
            IAlarmService alarmService,
            IMessageGroupService messageGroupService,
            IMemberService memberService,
            ILogger<HomeController> logger)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _alarmService = alarmService ?? throw new ArgumentNullException(nameof(alarmService));
            _messageGroupService = messageGroupService ?? throw new ArgumentNullException(nameof(messageGroupService));
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 메인 대시보드 화면을 표시합니다.
        /// </summary>
        /// <returns>대시보드 뷰</returns>
        public async Task<IActionResult> Index(string period = "weekly")
        {
            try
            {
                _logger.LogInformation("대시보드 화면 로드 시작: 기간={Period}", period);

                // 기간에 따른 날짜 범위 설정
                var (startDate, endDate) = GetDateRange(period);

                // 대시보드에 표시할 데이터 수집
                var dashboardData = new DashboardViewModel
                {
                    // 최근 메시지 목록 (최대 10개)
                    RecentMessages = await GetRecentMessagesAsync(10),

                    // 활성 알람 포인트 목록
                    ActiveAlarmPoints = await GetActiveAlarmPointsAsync(),

                    // 통계 정보
                    Statistics = await GetStatisticsAsync(startDate, endDate),

                    // 차트 데이터
                    ChartData = await GetChartDataAsync(period),

                    // 현재 선택된 기간
                    SelectedPeriod = period
                };

                _logger.LogInformation("대시보드 화면 로드 완료: 기간={Period}", period);
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "대시보드 화면 로드 중 오류 발생: {ErrorMessage}", ex.Message);

                // 오류 발생 시 빈 모델로 뷰 표시
                TempData["ErrorMessage"] = "대시보드 데이터를 불러오는 중 오류가 발생했습니다.";
                return View(new DashboardViewModel { SelectedPeriod = period });
            }
        }
        /// <summary>
        /// 선택된 기간에 따른 날짜 범위를 반환합니다.
        /// </summary>
        /// <param name="period">기간 (weekly, monthly, yearly)</param>
        /// <returns>시작 날짜와 종료 날짜</returns>
        private (DateTime startDate, DateTime endDate) GetDateRange(string period)
        {
            var endDate = DateTime.Now;
            DateTime startDate;

            switch (period.ToLower())
            {
                case "monthly":
                    startDate = endDate.AddMonths(-1);
                    break;
                case "yearly":
                    startDate = endDate.AddYears(-1);
                    break;
                case "weekly":
                default:
                    startDate = endDate.AddDays(-7);
                    break;
            }

            return (startDate, endDate);
        }

        /// <summary>
        /// 차트에 표시할 데이터를 가져옵니다.
        /// </summary>
        /// <param name="period">기간 (weekly, monthly, yearly)</param>
        /// <returns>차트 데이터</returns>
        private async Task<ChartData> GetChartDataAsync(string period)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(period);
                var messages = await _messageService.GetMessagesByDateRangeAsync(startDate, endDate);

                // null 체크 (방어적 프로그래밍)
                if (messages == null)
                {
                    _logger.LogWarning("메시지 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    messages = new List<Message>();
                }

                var chartData = new ChartData();

                // 기간에 따라 데이터 그룹화
                switch (period.ToLower())
                {
                    case "monthly":
                        // 일별 그룹화
                        chartData = GroupMessagesByDay(messages, startDate, endDate);
                        break;
                    case "yearly":
                        // 월별 그룹화
                        chartData = GroupMessagesByMonth(messages, startDate, endDate);
                        break;
                    case "weekly":
                    default:
                        // 시간별 그룹화
                        chartData = GroupMessagesByHour(messages, startDate, endDate);
                        break;
                }

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "차트 데이터 수집 중 오류 발생: {ErrorMessage}", ex.Message);
                return new ChartData();
            }
        }
        /// <summary>
        /// 메시지를 시간별로 그룹화합니다.
        /// </summary>
        private ChartData GroupMessagesByHour(IEnumerable<Message> messages, DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var successData = new List<int>();
            var failedData = new List<int>();

            // 시간별 데이터 초기화
            for (var date = startDate; date <= endDate; date = date.AddHours(4))
            {
                labels.Add(date.ToString("MM/dd HH:00"));

                var periodEnd = date.AddHours(4);
                var periodMessages = messages.Where(m => m.SendTime >= date && m.SendTime < periodEnd);

                successData.Add(periodMessages.Count(m => m.Status == "발송 완료"));
                failedData.Add(periodMessages.Count(m => m.Status == "발송 실패" || m.Status == "일부 발송 실패"));
            }

            return new ChartData
            {
                Labels = labels,
                SuccessData = successData,
                FailedData = failedData
            };
        }

        /// <summary>
        /// 메시지를 일별로 그룹화합니다.
        /// </summary>
        private ChartData GroupMessagesByDay(IEnumerable<Message> messages, DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var successData = new List<int>();
            var failedData = new List<int>();

            // 일별 데이터 초기화
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                labels.Add(date.ToString("MM/dd"));

                var dayMessages = messages.Where(m => m.SendTime.Date == date);

                successData.Add(dayMessages.Count(m => m.Status == "발송 완료"));
                failedData.Add(dayMessages.Count(m => m.Status == "발송 실패" || m.Status == "일부 발송 실패"));
            }

            return new ChartData
            {
                Labels = labels,
                SuccessData = successData,
                FailedData = failedData
            };
        }

        /// <summary>
        /// 메시지를 월별로 그룹화합니다.
        /// </summary>
        private ChartData GroupMessagesByMonth(IEnumerable<Message> messages, DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var successData = new List<int>();
            var failedData = new List<int>();

            // 월별 데이터 초기화
            for (var date = new DateTime(startDate.Year, startDate.Month, 1);
                 date <= new DateTime(endDate.Year, endDate.Month, 1);
                 date = date.AddMonths(1))
            {
                labels.Add(date.ToString("yyyy/MM"));

                var monthEnd = date.AddMonths(1);
                var monthMessages = messages.Where(m => m.SendTime >= date && m.SendTime < monthEnd);

                successData.Add(monthMessages.Count(m => m.Status == "발송 완료"));
                failedData.Add(monthMessages.Count(m => m.Status == "발송 실패" || m.Status == "일부 발송 실패"));
            }

            return new ChartData
            {
                Labels = labels,
                SuccessData = successData,
                FailedData = failedData
            };
        }

    /// <summary>
    /// 최근 메시지 목록을 조회합니다.
    /// </summary>
    /// <param name="count">조회할 메시지 개수</param>
    /// <returns>최근 메시지 목록</returns>
    private async Task<IEnumerable<Message>> GetRecentMessagesAsync(int count)
        {
            try
            {
                // 최근 30일 데이터 조회
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);

                var messages = await _messageService.GetMessagesByDateRangeAsync(startDate, endDate);

                // null 체크 (방어적 프로그래밍)
                if (messages == null)
                {
                    _logger.LogWarning("메시지 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<Message>();
                }

                // 최신순으로 정렬하여 지정된 개수만큼 반환
                return messages.OrderByDescending(m => m.SendTime).Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "최근 메시지 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<Message>();
            }
        }

        /// <summary>
        /// 활성화된 알람 포인트 목록을 조회합니다.
        /// </summary>
        /// <returns>활성화된 알람 포인트 목록</returns>
        private async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                var alarmPoints = await _alarmService.GetActiveAlarmPointsAsync();

                // null 체크 (방어적 프로그래밍)
                if (alarmPoints == null)
                {
                    _logger.LogWarning("알람 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<AlarmPoint>();
                }

                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성 알람 포인트 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<AlarmPoint>();
            }
        }

        /// <summary>
        /// 시스템 통계 정보를 수집합니다.
        /// </summary>
        /// <param name="startDate">통계 시작 날짜</param>
        /// <param name="endDate">통계 종료 날짜</param>
        /// <returns>통계 정보</returns>
        private async Task<DashboardStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 기간 내 메시지 조회
                var messages = await _messageService.GetMessagesByDateRangeAsync(startDate, endDate);

                // 그룹, 회원, 알람 포인트 조회
                var groups = await _messageGroupService.GetAllGroupsAsync();
                var members = await _memberService.GetAllMembersAsync();
                var alarmPoints = await _alarmService.GetAllAlarmPointsAsync();

                // null 체크 (방어적 프로그래밍)
                if (messages == null) messages = new List<Message>();
                if (groups == null) groups = new List<MessageGroup>();
                if (members == null) members = new List<Member>();
                if (alarmPoints == null) alarmPoints = new List<AlarmPoint>();

                // 통계 정보 계산
                var statistics = new DashboardStatistics
                {
                    TotalMessages = messages.Count(),
                    SuccessMessages = messages.Count(m => m.Status == "발송 완료"),
                    FailedMessages = messages.Count(m => m.Status == "발송 실패"),
                    PartialMessages = messages.Count(m => m.Status == "일부 발송 실패"),

                    TotalGroups = groups.Count(),
                    ActiveGroups = groups.Count(g => g.IsActive),

                    TotalMembers = members.Count(),
                    ActiveMembers = members.Count(m => m.IsActive),

                    TotalAlarmPoints = alarmPoints.Count(),
                    ActiveAlarmPoints = alarmPoints.Count(a => a.IsActive)
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "통계 정보 수집 중 오류 발생: {ErrorMessage}", ex.Message);

                // 오류 발생 시 기본값 반환
                return new DashboardStatistics();
            }
        }
    }

    /// <summary>
    /// 대시보드 화면에 표시할 데이터를 담는 뷰 모델
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// 최근 메시지 목록
        /// </summary>
        public IEnumerable<Message> RecentMessages { get; set; } = new List<Message>();

        /// <summary>
        /// 활성화된 알람 포인트 목록
        /// </summary>
        public IEnumerable<AlarmPoint> ActiveAlarmPoints { get; set; } = new List<AlarmPoint>();

        /// <summary>
        /// 통계 정보
        /// </summary>
        public DashboardStatistics Statistics { get; set; } = new DashboardStatistics();

        /// <summary>
        /// 차트 데이터
        /// </summary>
        public ChartData ChartData { get; set; } = new ChartData();

        /// <summary>
        /// 현재 선택된 기간 (weekly, monthly, yearly)
        /// </summary>
        public string SelectedPeriod { get; set; } = "weekly";
    }
    /// <summary>
    /// 차트에 표시할 데이터
    /// </summary>
    public class ChartData
    {
        /// <summary>
        /// X축 라벨
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// 성공 메시지 데이터
        /// </summary>
        public List<int> SuccessData { get; set; } = new List<int>();

        /// <summary>
        /// 실패 메시지 데이터
        /// </summary>
        public List<int> FailedData { get; set; } = new List<int>();
    }
    /// <summary>
    /// 대시보드에 표시할 통계 정보
    /// </summary>
    public class DashboardStatistics
    {
        // 메시지 관련 통계
        public int TotalMessages { get; set; } = 0;
        public int SuccessMessages { get; set; } = 0;
        public int FailedMessages { get; set; } = 0;
        public int PartialMessages { get; set; } = 0;

        // 그룹 관련 통계
        public int TotalGroups { get; set; } = 0;
        public int ActiveGroups { get; set; } = 0;

        // 회원 관련 통계
        public int TotalMembers { get; set; } = 0;
        public int ActiveMembers { get; set; } = 0;

        // 알람 포인트 관련 통계
        public int TotalAlarmPoints { get; set; } = 0;
        public int ActiveAlarmPoints { get; set; } = 0;
    }
}

// �ۼ���: Sangchan, Kim
// �ۼ���: 2025-04-01
// ���: ���� ��ú��� ��Ʈ�ѷ�
// ����: SMS �˶� �ý����� ���� ��ú��� ȭ���� �����ϴ� ��Ʈ�ѷ��Դϴ�.

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
    /// ���� ��ú��� ȭ���� �����ϴ� ��Ʈ�ѷ�
    /// �ý��� ����, ��� ���� �� �ֱ� Ȱ���� ǥ���մϴ�.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IAlarmService _alarmService;
        private readonly IMessageGroupService _messageGroupService;
        private readonly IMemberService _memberService;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// HomeController ������
        /// </summary>
        /// <param name="messageService">�޽��� ���� �ν��Ͻ�</param>
        /// <param name="alarmService">�˶� ���� �ν��Ͻ�</param>
        /// <param name="messageGroupService">�޽��� �׷� ���� �ν��Ͻ�</param>
        /// <param name="memberService">ȸ�� ���� �ν��Ͻ�</param>
        /// <param name="logger">�α��� ���� ILogger �ν��Ͻ�</param>
        /// <exception cref="ArgumentNullException">�ʼ� �Ű������� null�� ��� �߻�</exception>
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
        /// ���� ��ú��� ȭ���� ǥ���մϴ�.
        /// </summary>
        /// <returns>��ú��� ��</returns>
        public async Task<IActionResult> Index(string period = "weekly")
        {
            try
            {
                _logger.LogInformation("��ú��� ȭ�� �ε� ����: �Ⱓ={Period}", period);

                // �Ⱓ�� ���� ��¥ ���� ����
                var (startDate, endDate) = GetDateRange(period);

                // ��ú��忡 ǥ���� ������ ����
                var dashboardData = new DashboardViewModel
                {
                    // �ֱ� �޽��� ��� (�ִ� 10��)
                    RecentMessages = await GetRecentMessagesAsync(10),

                    // Ȱ�� �˶� ����Ʈ ���
                    ActiveAlarmPoints = await GetActiveAlarmPointsAsync(),

                    // ��� ����
                    Statistics = await GetStatisticsAsync(startDate, endDate),

                    // ��Ʈ ������
                    ChartData = await GetChartDataAsync(period),

                    // ���� ���õ� �Ⱓ
                    SelectedPeriod = period
                };

                _logger.LogInformation("��ú��� ȭ�� �ε� �Ϸ�: �Ⱓ={Period}", period);
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��ú��� ȭ�� �ε� �� ���� �߻�: {ErrorMessage}", ex.Message);

                // ���� �߻� �� �� �𵨷� �� ǥ��
                TempData["ErrorMessage"] = "��ú��� �����͸� �ҷ����� �� ������ �߻��߽��ϴ�.";
                return View(new DashboardViewModel { SelectedPeriod = period });
            }
        }
        /// <summary>
        /// ���õ� �Ⱓ�� ���� ��¥ ������ ��ȯ�մϴ�.
        /// </summary>
        /// <param name="period">�Ⱓ (weekly, monthly, yearly)</param>
        /// <returns>���� ��¥�� ���� ��¥</returns>
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
        /// ��Ʈ�� ǥ���� �����͸� �����ɴϴ�.
        /// </summary>
        /// <param name="period">�Ⱓ (weekly, monthly, yearly)</param>
        /// <returns>��Ʈ ������</returns>
        private async Task<ChartData> GetChartDataAsync(string period)
        {
            try
            {
                var (startDate, endDate) = GetDateRange(period);
                var messages = await _messageService.GetMessagesByDateRangeAsync(startDate, endDate);

                // null üũ (����� ���α׷���)
                if (messages == null)
                {
                    _logger.LogWarning("�޽��� ���񽺿��� null ��ȯ��. �� ������� ��ü�մϴ�.");
                    messages = new List<Message>();
                }

                var chartData = new ChartData();

                // �Ⱓ�� ���� ������ �׷�ȭ
                switch (period.ToLower())
                {
                    case "monthly":
                        // �Ϻ� �׷�ȭ
                        chartData = GroupMessagesByDay(messages, startDate, endDate);
                        break;
                    case "yearly":
                        // ���� �׷�ȭ
                        chartData = GroupMessagesByMonth(messages, startDate, endDate);
                        break;
                    case "weekly":
                    default:
                        // �ð��� �׷�ȭ
                        chartData = GroupMessagesByHour(messages, startDate, endDate);
                        break;
                }

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��Ʈ ������ ���� �� ���� �߻�: {ErrorMessage}", ex.Message);
                return new ChartData();
            }
        }
        /// <summary>
        /// �޽����� �ð����� �׷�ȭ�մϴ�.
        /// </summary>
        private ChartData GroupMessagesByHour(IEnumerable<Message> messages, DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var successData = new List<int>();
            var failedData = new List<int>();

            // �ð��� ������ �ʱ�ȭ
            for (var date = startDate; date <= endDate; date = date.AddHours(4))
            {
                labels.Add(date.ToString("MM/dd HH:00"));

                var periodEnd = date.AddHours(4);
                var periodMessages = messages.Where(m => m.SendTime >= date && m.SendTime < periodEnd);

                successData.Add(periodMessages.Count(m => m.Status == "�߼� �Ϸ�"));
                failedData.Add(periodMessages.Count(m => m.Status == "�߼� ����" || m.Status == "�Ϻ� �߼� ����"));
            }

            return new ChartData
            {
                Labels = labels,
                SuccessData = successData,
                FailedData = failedData
            };
        }

        /// <summary>
        /// �޽����� �Ϻ��� �׷�ȭ�մϴ�.
        /// </summary>
        private ChartData GroupMessagesByDay(IEnumerable<Message> messages, DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var successData = new List<int>();
            var failedData = new List<int>();

            // �Ϻ� ������ �ʱ�ȭ
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                labels.Add(date.ToString("MM/dd"));

                var dayMessages = messages.Where(m => m.SendTime.Date == date);

                successData.Add(dayMessages.Count(m => m.Status == "�߼� �Ϸ�"));
                failedData.Add(dayMessages.Count(m => m.Status == "�߼� ����" || m.Status == "�Ϻ� �߼� ����"));
            }

            return new ChartData
            {
                Labels = labels,
                SuccessData = successData,
                FailedData = failedData
            };
        }

        /// <summary>
        /// �޽����� ������ �׷�ȭ�մϴ�.
        /// </summary>
        private ChartData GroupMessagesByMonth(IEnumerable<Message> messages, DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var successData = new List<int>();
            var failedData = new List<int>();

            // ���� ������ �ʱ�ȭ
            for (var date = new DateTime(startDate.Year, startDate.Month, 1);
                 date <= new DateTime(endDate.Year, endDate.Month, 1);
                 date = date.AddMonths(1))
            {
                labels.Add(date.ToString("yyyy/MM"));

                var monthEnd = date.AddMonths(1);
                var monthMessages = messages.Where(m => m.SendTime >= date && m.SendTime < monthEnd);

                successData.Add(monthMessages.Count(m => m.Status == "�߼� �Ϸ�"));
                failedData.Add(monthMessages.Count(m => m.Status == "�߼� ����" || m.Status == "�Ϻ� �߼� ����"));
            }

            return new ChartData
            {
                Labels = labels,
                SuccessData = successData,
                FailedData = failedData
            };
        }

    /// <summary>
    /// �ֱ� �޽��� ����� ��ȸ�մϴ�.
    /// </summary>
    /// <param name="count">��ȸ�� �޽��� ����</param>
    /// <returns>�ֱ� �޽��� ���</returns>
    private async Task<IEnumerable<Message>> GetRecentMessagesAsync(int count)
        {
            try
            {
                // �ֱ� 30�� ������ ��ȸ
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);

                var messages = await _messageService.GetMessagesByDateRangeAsync(startDate, endDate);

                // null üũ (����� ���α׷���)
                if (messages == null)
                {
                    _logger.LogWarning("�޽��� ���񽺿��� null ��ȯ��. �� ������� ��ü�մϴ�.");
                    return new List<Message>();
                }

                // �ֽż����� �����Ͽ� ������ ������ŭ ��ȯ
                return messages.OrderByDescending(m => m.SendTime).Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�ֱ� �޽��� ��ȸ �� ���� �߻�: {ErrorMessage}", ex.Message);
                return new List<Message>();
            }
        }

        /// <summary>
        /// Ȱ��ȭ�� �˶� ����Ʈ ����� ��ȸ�մϴ�.
        /// </summary>
        /// <returns>Ȱ��ȭ�� �˶� ����Ʈ ���</returns>
        private async Task<IEnumerable<AlarmPoint>> GetActiveAlarmPointsAsync()
        {
            try
            {
                var alarmPoints = await _alarmService.GetActiveAlarmPointsAsync();

                // null üũ (����� ���α׷���)
                if (alarmPoints == null)
                {
                    _logger.LogWarning("�˶� ���񽺿��� null ��ȯ��. �� ������� ��ü�մϴ�.");
                    return new List<AlarmPoint>();
                }

                return alarmPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ȱ�� �˶� ����Ʈ ��ȸ �� ���� �߻�: {ErrorMessage}", ex.Message);
                return new List<AlarmPoint>();
            }
        }

        /// <summary>
        /// �ý��� ��� ������ �����մϴ�.
        /// </summary>
        /// <param name="startDate">��� ���� ��¥</param>
        /// <param name="endDate">��� ���� ��¥</param>
        /// <returns>��� ����</returns>
        private async Task<DashboardStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // �Ⱓ �� �޽��� ��ȸ
                var messages = await _messageService.GetMessagesByDateRangeAsync(startDate, endDate);

                // �׷�, ȸ��, �˶� ����Ʈ ��ȸ
                var groups = await _messageGroupService.GetAllGroupsAsync();
                var members = await _memberService.GetAllMembersAsync();
                var alarmPoints = await _alarmService.GetAllAlarmPointsAsync();

                // null üũ (����� ���α׷���)
                if (messages == null) messages = new List<Message>();
                if (groups == null) groups = new List<MessageGroup>();
                if (members == null) members = new List<Member>();
                if (alarmPoints == null) alarmPoints = new List<AlarmPoint>();

                // ��� ���� ���
                var statistics = new DashboardStatistics
                {
                    TotalMessages = messages.Count(),
                    SuccessMessages = messages.Count(m => m.Status == "�߼� �Ϸ�"),
                    FailedMessages = messages.Count(m => m.Status == "�߼� ����"),
                    PartialMessages = messages.Count(m => m.Status == "�Ϻ� �߼� ����"),

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
                _logger.LogError(ex, "��� ���� ���� �� ���� �߻�: {ErrorMessage}", ex.Message);

                // ���� �߻� �� �⺻�� ��ȯ
                return new DashboardStatistics();
            }
        }
    }

    /// <summary>
    /// ��ú��� ȭ�鿡 ǥ���� �����͸� ��� �� ��
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// �ֱ� �޽��� ���
        /// </summary>
        public IEnumerable<Message> RecentMessages { get; set; } = new List<Message>();

        /// <summary>
        /// Ȱ��ȭ�� �˶� ����Ʈ ���
        /// </summary>
        public IEnumerable<AlarmPoint> ActiveAlarmPoints { get; set; } = new List<AlarmPoint>();

        /// <summary>
        /// ��� ����
        /// </summary>
        public DashboardStatistics Statistics { get; set; } = new DashboardStatistics();

        /// <summary>
        /// ��Ʈ ������
        /// </summary>
        public ChartData ChartData { get; set; } = new ChartData();

        /// <summary>
        /// ���� ���õ� �Ⱓ (weekly, monthly, yearly)
        /// </summary>
        public string SelectedPeriod { get; set; } = "weekly";
    }
    /// <summary>
    /// ��Ʈ�� ǥ���� ������
    /// </summary>
    public class ChartData
    {
        /// <summary>
        /// X�� ��
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// ���� �޽��� ������
        /// </summary>
        public List<int> SuccessData { get; set; } = new List<int>();

        /// <summary>
        /// ���� �޽��� ������
        /// </summary>
        public List<int> FailedData { get; set; } = new List<int>();
    }
    /// <summary>
    /// ��ú��忡 ǥ���� ��� ����
    /// </summary>
    public class DashboardStatistics
    {
        // �޽��� ���� ���
        public int TotalMessages { get; set; } = 0;
        public int SuccessMessages { get; set; } = 0;
        public int FailedMessages { get; set; } = 0;
        public int PartialMessages { get; set; } = 0;

        // �׷� ���� ���
        public int TotalGroups { get; set; } = 0;
        public int ActiveGroups { get; set; } = 0;

        // ȸ�� ���� ���
        public int TotalMembers { get; set; } = 0;
        public int ActiveMembers { get; set; } = 0;

        // �˶� ����Ʈ ���� ���
        public int TotalAlarmPoints { get; set; } = 0;
        public int ActiveAlarmPoints { get; set; } = 0;
    }
}

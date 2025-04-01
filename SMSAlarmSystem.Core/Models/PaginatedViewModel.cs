// 작성자: Sangchan, Kim
// 작성일: 2025-04-01
// 기능: 페이지네이션을 위한 뷰 모델
// 설명: 페이지네이션 정보와 데이터를 함께 담는 제네릭 클래스입니다.

namespace SMSAlarmSystem.Models
{
    /// <summary>
    /// 페이지네이션 정보와 데이터를 담는 제네릭 뷰 모델
    /// </summary>
    /// <typeparam name="T">페이지네이션할 데이터 타입</typeparam>
    public class PaginatedViewModel<T> where T : class
    {
        /// <summary>
        /// 현재 페이지의 아이템 목록
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 현재 페이지 번호 (1부터 시작)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 페이지당 아이템 수
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 전체 아이템 수
        /// </summary>
        public int TotalItems { get; set; } = 0;

        /// <summary>
        /// 전체 페이지 수
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        /// <summary>
        /// 이전 페이지가 있는지 여부
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// 다음 페이지가 있는지 여부
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}

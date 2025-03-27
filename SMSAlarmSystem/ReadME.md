# SMS 알람 시스템

## 프로젝트 개요

SMS 알람 시스템은 알람 포인트 기반 단체 문자 발송 기능을 제공하는 웹 애플리케이션입니다. 회원 그룹, 단체 문자 그룹, 알람 포인트를 관리하고, 특정 조건이 충족될 때 지정된 그룹에 SMS를 발송합니다.

## 주요 기능

- 알람 포인트 기반 단체 문자 발송
- 회원 그룹, 단체 문자 그룹, 알람 포인트 CRUD 기능
- 실시간 문자 발송 내역 조회
- 권한별 접근 제어
- 회원 활동 로그 저장

## 기술 스택

- **프론트엔드**: ASP.NET MVC, HTML, CSS (Tailwind CSS), JavaScript
- **백엔드**: C#, ASP.NET Core
- **데이터베이스**: MSSQL 2022
- **ORM**: Entity Framework Core
- **로깅**: Serilog
- **의존성 주입**: ASP.NET Core 내장 DI 컨테이너

## 프로젝트 구조

- **SMSAlarmSystem.Core**: 핵심 도메인 모델, 인터페이스
- **SMSAlarmSystem.Infrastructure**: 데이터 액세스, 외부 서비스 통합
- **SMSAlarmSystem.Services**: 비즈니스 로직
- **SMSAlarmSystem**: 웹 애플리케이션 (컨트롤러, 뷰)
- **SMSAlarmSystem.Tests**: 단위 테스트

## 시작하기

### 필수 조건

- Visual Studio 2022
- .NET 6.0 이상
- MSSQL Server 2022

### 설치 및 실행

1. 저장소 복제

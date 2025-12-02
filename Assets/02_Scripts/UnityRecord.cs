using Newtonsoft.Json;
using UnityEngine;

// record 키워드를 사용하려면 C# 9.0 이상이 필요합니다.
// 유니티에서 record 키워드를 사용하려면 아래 네임스페이스와 함께 클래스 정의를 필요로 합니다.
// 아래 클래스가 없으면 컴파일 에러와 함께 record 키워드를 사용할 수 없습니다.
namespace System.Runtime.CompilerServices {
    public class IsExternalInit {
        
    }
}

namespace CoffeeCat.Research {
    public class UnityRecord : MonoBehaviour {
        // record 를 사용하는 이유는 데이터 전달을 위한 클래스를 간단하게 만들기 위함입니다.
        // record 키워드를 사용하면 클래스의 생성자, 프로퍼티, ToString, Equals, GetHashCode 등을 자동으로 생성해줍니다.
        
        // 테스트를 위한 임시 record를 생성합니다 
        public record SampleRecord(string Name, string Desc);
        
        // 1. 데이터 불변성 지원 (Immutable Object)
        // record는 기본적으로 불변(immutable) 데이터를 표현하기에 적합합니다.
        // 생성된 객체의 속성은 기본적으로 읽기 전용(readonly)이며, 값이 변경되지 않도록 보장합니다.
        // 이를 통해 예기치 않은 데이터 변경을 방지하고 멀티스레드 환경에서 안전한 작업을 수행할 수 있습니다.
        public void ImmutableObject() {
            // SampleRecord 객체 생성
            var sampleRecord = new SampleRecord("name", "desc");
            
            // record 객체의 속성은 읽기 전용이기 때문에 값을 변경할 수 없습니다.
            // record.Name = "Changed"; // 컴파일 에러 발생
            
            // 값을 변경하려면 새 인스턴스를 생성해야 함 (Immutable)
            var updatedRecord = sampleRecord with { Desc = "new desc" };

            Debug.Log(sampleRecord);  // 출력: sampleRecord  { Name = name, Desc = desc }
            Debug.Log(updatedRecord); // 출력: updatedRecord { Name = name, Desc = new desc }
        }
        
        // 2. 값 기반 비교 (Value-based Equality)
        // record는 객체의 값을 기준으로 비교를 수행합니다.
        // 일반 클래스(class)는 **참조 기반 비교(Reference-based Equality)**를 수행하지만, record는 속성의 값이 같다면 두 객체를 동일하다고 간주합니다.
        public record Point(int X, int Y);

        public void ValueBasedEquality() {
            var p1 = new Point(3, 5);
            var p2 = new Point(3, 5);

            Debug.Log(p1 == p2); // true: 값이 같으므로 동일
        }
        
        // 3. with 키워드를 통한 간편한 복사
        // record 객체는 불변이지만, 특정 속성만 변경된 새 객체를 쉽게 생성할 수 있습니다.
        // 이를 통해 데이터 수정 작업이 간결해집니다.
        public void WithKeyword() {
            var original = new Point(3, 5);
            var modified = original with { Y = 10 };

            Debug.Log(original); // 출력: Point { X = 3, Y = 5 }
            Debug.Log(modified); // 출력: Point { X = 3, Y = 10 }
        }
        
        // 4. 간결한 선언
        // record는 보일러플레이트 코드를 줄여줍니다. 생성자, 속성, ToString, GetHashCode, Equals를 자동으로 생성합니다.
        // class로 동일한 작업을 수행하려면 수동으로 작성해야 하는 코드를 크게 줄일 수 있습니다.
        
        // 5. 데이터 중심 모델링
        // record는 엔티티(Entity)나 데이터 전송 객체(DTO)를 정의할 때 유용합니다.
        // Unity 프로젝트에서 다음과 같은 작업에 사용됩니다:
        // 게임 상태 관리: 플레이어 데이터, 게임 설정 등을 불변 객체로 유지.
        // 세이브 데이터: JSON 직렬화/역직렬화 시 유용.

        // 6. 효율적인 직렬화 지원
        // record 객체는 JSON이나 XML로 직렬화할 때 유용합니다. 속성 정의와 기본 생성자가 포함되어 있어 데이터 모델링에 적합합니다.
        // Unity에서 JSON으로 저장/로드하는 경우 record를 사용하면 편리합니다.

        public record PlayerState(int Level, int Health);
        
        public void Serialization() {
            var state = new PlayerState(2, 90);

            // 직렬화
            string json = JsonConvert.SerializeObject(state);
            Debug.Log(json); // 출력: {"Level":2,"Health":90}
            
            
            // 역직렬화
            var deserialized = JsonConvert.DeserializeObject<PlayerState>(json);
            Debug.Log(deserialized); // 출력: PlayerState { Level = 2, Health = 90 }
        }
        
        // 7. 패턴 매칭 지원
        // record는 패턴 매칭과 잘 어울립니다. 복잡한 조건에서 데이터 구조를 쉽게 비교할 수 있습니다.
        public record GameEvent(string EventType, int Value);
        
        public void PatternMatching() {
            var gameEvent = new GameEvent("Damage", 50);

            if (gameEvent is GameEvent("Damage", int damage))
            {
                Debug.Log($"플레이어가 {damage}의 피해를 입었습니다.");
            }
        }
        
        // record의 한계
        // record는 기본적으로 참조 타입입니다. 값 타입(struct)과 같이 스택에 저장되길 원한다면 record struct를 사용해야 합니다.
        // Unity의 Inspector에서 record 타입을 드래그 앤 드롭하거나 수정하는 것은 기본적으로 지원되지 않으므로,
        // Unity의 ScriptableObject와 같은 데이터 구조와 혼합 사용이 필요할 수 있습니다.
        
        // 결론 C#의 record는 데이터 중심의 프로그래밍에서 많은 장점을 제공합니다. 특히, Unity 프로젝트에서는 다음과 같은 장점이 돋보입니다:
        // 불변 데이터 구조로 멀티스레드 환경에서 안전하게 사용.
        // 값 기반 비교로 데이터 동등성 검사 간소화.
        // JSON 등 데이터 직렬화/역직렬화에 적합.
        // 간결한 코드로 유지보수 용이.
        // Unity에서 데이터 모델링, 상태 관리, 세이브 시스템에 활용하면 생산성과 코드의 간결성을 크게 향상시킬 수 있습니다.
    }
}
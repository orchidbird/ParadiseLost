using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public static class SkillLogicFactory{
	public static BaseActiveSkillLogic Get(ActiveSkill skill){
        BaseActiveSkillLogic skillLogic;
			switch (skill.GetName()) {

			// AI
			case "돌진":
				skillLogic = new BasicChargeSkillLogic ();
				break;
			case "숲의 은총":
				skillLogic = new S81_SkillLogic2();
				break;
			case "대지의 비늘":	//10/15스테이지 수인족 장로
				skillLogic = new S101_SkillLogic ();
				break;
			case "방패치기":
				skillLogic = new Stage_16_0_SkillLogic ();
				break;
			case "칸티움의 울타리":
				skillLogic = new S161_Monk_Shield_SkillLogic();
				break;
			case "혼비백산":
				skillLogic = new Stage_18_0_SkillLogic();
				break;
			case "찬송":
				skillLogic = new S181_SkillLogic();
				break;
			case "눈 먹기":
				skillLogic = new S191_0_SkillLogic();
				break;
			case "얼음 함성":
				skillLogic = new Stage_20_1_SkillLogic();
				break;
			case "높은 도약":
				skillLogic = new Stage_20_2_SkillLogic();
				break;

            // 그레네브
			case "저격":
				skillLogic = new Grenev_1_l_SkillLogic ();
            break;
            case "단검술":
				skillLogic = new Grenev_B1_SkillLogic();
            break;
            case "집중":
                skillLogic = new Grenev_2_l_SkillLogic();
            break;
            case "암살 표식":
                skillLogic = new Grenev_3_m_SkillLogic();
            break;
            case "회심의 일격":
                skillLogic = new Grenev_4_l_SkillLogic();
            break;
            case "은신":
                skillLogic = new Grenev_5_m_SkillLogic();
            break;
            case "연사":
                skillLogic = new Grenev_7_l_SkillLogic();
            break;
            case "최후의 일격":
                skillLogic = new Grenev_8_l_SkillLogic();
            break;

            // 노엘
            case "파마의 섬광":
            skillLogic = new Noel_A1_SkillLogic();
            break;
            case "신의 가호":
				skillLogic = new Noel_1_r_SkillLogic();
            break;
            case "언령":
                skillLogic = new Noel_2_m_SkillLogic();
            break;
            case "신의 손길":
                skillLogic = new Noel_2_r_SkillLogic();
            break;

            // 달케니르
            case "에테르 샷":
				skillLogic = new Darkenir_EtherShot_SkillLogic();
            break;
            case "중력 과부하":
                skillLogic = new Darkenir_2_r_SkillLogic();
            break;

			// 데우스
			case "명상":
				skillLogic = new Deus_1_m_SkillLogic();
			break;

			// 레이나
			case "화염 폭발":
				skillLogic = new Reina_A1_SkillLogic();
            break;
			case "화염구":
				skillLogic = new Reina_1_m_SkillLogic();
            break;
			case "마력 연쇄":
			    skillLogic = new Reina_C15_SkillLogic();
            break;
			case "지옥 불꽃":
            skillLogic = new Reina_4_m_SkillLogic();
            break;
            // case "에테르 과부하":
            // return new Reina_5_r_SkillLogic();

			//루키어스
			case "푸른 검격":
				skillLogic = new Lucius_BlueSlash_SkillLogic();
			break;
            case "쇄도":
                skillLogic = new Lucius_C1_SkillLogic();
            break;

            // 루베리카
            case "교향곡: 운명":
            skillLogic = new Luvericha_Symphony_SkillLogic();
            break;
            case "사랑의 기쁨":
            skillLogic = new Luvericha_1_r_SkillLogic();
            break;
            case "에튀드: 겨울바람":
            skillLogic = new Luvericha_4_l_SkillLogic();
            break;
            case "에튀드: 햇빛":
            skillLogic = new Luvericha_C22_SkillLogic();
            break;
            case "소녀의 기도":
            skillLogic = new Luvericha_5_m_SkillLogic();
            break;
            case "정화된 밤":
            skillLogic = new Luvericha_6_l_SkillLogic();
            break;
            case "4분 33초":
            skillLogic = new Luvericha_8_l_SkillLogic();
            break;
            case "환상곡":
            skillLogic = new Luvericha_C50_SkillLogic();
            break;

			// 리니안
			case "하전 파동":
				skillLogic = new Lenien_A1_SkillLogic();
				break;
			case "전자기 충격": case "전자기 충격_test":
				skillLogic = new Lenien_1_m_SkillLogic();
				break;
			case "자기장 형성":
				skillLogic = new Lenien_C1_SkillLogic();
				break;
			case "축전":
            skillLogic = new Lenien_7_l_SkillLogic();
            break;

            //비앙카
            case "잘근잘근 덫":
            skillLogic = new Bianca_ChewingTrap_SkillLogic();
            break;
            case "떠밀기":
            skillLogic = new Bianca_1_r_SkillLogic();
            break;
            case "짜릿짜릿 지뢰":
            skillLogic = new Bianca_A8_SkillLogic();
            break;
            case "부패진":
            skillLogic = new Bianca_B8_SkillLogic();
            break;

            //세피아
            case "반달베기":
				skillLogic = new Sepia_HalfMoon_SkillLogic();
            break;
            case "무아베기":
                skillLogic = new Sepia_A8_SkillLogic();
            break;
            case "봉쇄격":
                skillLogic = new Sepia_Blockade_SkillLogic();
            break;

            //아르카디아
            case "생명의 요람":
            skillLogic = new Arcadia_1_m_SkillLogic();
            break;
            case "계절풍":
            skillLogic = new Arcadia_C1_SkillLogic();
            break;
            case "풍화":
            skillLogic = new Arcadia_Weathering_SkillLogic();
            break;

			// 영
			case "은빛 베기": case "은빛 베기_test":
				skillLogic = new Yeong_1_l_SkillLogic();
            break;
			case "섬광 찌르기":
            skillLogic = new Yeong_2_l_SkillLogic();
            break;
			case "초감각":
            skillLogic = new Yeong_5_r_SkillLogic();
            break;

			// 에렌
			case "칠흑의 화살":
            skillLogic = new Eren_DarkArrow_SkillLogic();
            break;
			case "광휘": case "광휘_test":
            skillLogic = new Eren_C1_SkillLogic();
            break;
			case "죽음의 화살": case "죽음의 화살_test":
            skillLogic = new Eren_3_l_SkillLogic();
            break;
			case "치유의 빛":
            skillLogic = new Eren_6_r_SkillLogic();
            break;

            //유진
            case "얼음 파편":
            skillLogic = new Eugene_1_l_SkillLogic();
            break;
            case "순백의 방패":
            skillLogic = new AttachAura();
            break;
            case "거울 방패":
            skillLogic = new Eugene_3_m_SkillLogic();
            break;
            case "얼음의 가호":
            skillLogic = new Eugene_4_l_SkillLogic();
            break;
            case "백은의 장막":
            skillLogic = new Eugene_4_m_SkillLogic();
            break;
            case "겨울의 가호":
            skillLogic = new Eugene_8_l_SkillLogic();
            break;
            case "백은의 오로라":
            skillLogic = new Eugene_B50_SkillLogic();
            break;

			//라티스
			case "접근 거부":
				skillLogic = new Ratice_1_m_SkillLogic ();
				break;
			case "제압":
				skillLogic = new Ratice_1_r_SkillLogic ();
				break;
			case "보호의 손길":
				skillLogic = new Ratice_2_l_SkillLogic ();
				break;

            //제이선
            case "그림자 일격":
            skillLogic = new Json_Shadow_SkillLogic();
            break;
            case "단검 투척":
            skillLogic = new Json_Dagger_SkillLogic();
            break;

			// 카샤스티
			case "더블 샷":
            skillLogic = new Kashasty_DoubleShot_SkillLogic();
            break;
            case "셔플 불릿":
            skillLogic = new Kashasty_B15_SkillLogic();
            break;

            //칼드리치
            case "죽창 찌르기":
            skillLogic = new Karldrich_1_l_SkillLogic();
            break;

            //트리아나
            case "열풍":
            skillLogic = new Triana_HeatWave_SkillLogic();
            break;
            case "얼음날":
            skillLogic = new Triana_IceBlade_SkillLogic();
            break;
            case "포획":
            skillLogic = new Triana_Capture_SkillLogic();
            break;
            case "결정화":
            skillLogic = new Triana_Crystalize_SkillLogic();
            break;
            
            //큐리
            case "수상한 덩어리":
            skillLogic = new Curi_FlammableAttachment_SkillLogic();
            break;
            case "동기화 점액":
            skillLogic = new Curi_4_l_SkillLogic();
            break;
            case "산성 혼합물":
            skillLogic = new Curi_B22_SkillLogic();
            break;
            case "인간 중정석 폭탄":
            skillLogic = new Curi_4_r_SkillLogic();
            break;
            case "유독성 촉매":
            skillLogic = new Curi_6_m_SkillLogic();
            break;
            case "에테르 폭탄":
            skillLogic = new Curi_C43_SkillLogic();
            break;
            case "초강산 혼합물":
            skillLogic = new Curi_8_m_SkillLogic();
            break;

            default:
            skillLogic = new BaseActiveSkillLogic();
            break;
		}
        skillLogic.activeSkill = skill;
		skillLogic.skill = skill;
        return skillLogic;
	}

	public static ListPassiveSkillLogic Get(List<PassiveSkill> passiveSkills){
		return new ListPassiveSkillLogic(passiveSkills.ConvertAll(Get));
	}

	public static BasePassiveSkillLogic Get(PassiveSkill passiveSkill){
		BasePassiveSkillLogic passiveSkillLogic = null;
		switch (passiveSkill.korName){
            // AI
			case "파편":
				passiveSkillLogic = new FragmentSkillLogic();
				break;
			case "광폭화":
				passiveSkillLogic = new S31_SkillLogic();
				break;
			case "약점 노출":
				passiveSkillLogic = new S31_Cavalry_SkillLogic();
				break;
            case "도망자":
            	passiveSkillLogic = new S51_SkillLogic();
            	break;
			case "완강한 복족류":
				passiveSkillLogic = new S61_SkillLogic0();
				break;
			case "흉포함":
				passiveSkillLogic = new S61_SkillLogic1();
				break;
            case "생명의 갑옷":
            	passiveSkillLogic = new S81_SkillLogic0();
            	break;
			case "생명 연결":
				passiveSkillLogic = new S81_Link_SkillLogic();
				break;
			case "에테르 공급":
				passiveSkillLogic = new AttachAuraOnStart();
				break;
			case "잠금":
				passiveSkillLogic = new AttachOnStart();
				break;
			case "군수 전문가":
				passiveSkillLogic = new S102_eichmann_SkillLogic0();
				break;
			case "연막탄":
			passiveSkillLogic = new S102_eichmann_SkillLogic1();
			break;
            case "나를 따르라":
            	passiveSkillLogic = new S131_Sage_Leader_SkillLogic();
            	break;
			case "화합물 정제":
				passiveSkillLogic = new S131_Sage_Compound_SkillLogic();
				break;
            case "살아움직이는 갑옷":
	            passiveSkillLogic = new S141_Armor_SkillLogic();
	            break;
            case "영체":
	            passiveSkillLogic = new S161_Spirit_Nonmaterial_SkillLogic();
	            break;
			case "점멸":
				passiveSkillLogic = new S161_Spirit_Flash_SkillLogic();
				break;
            case "폭주":
	            passiveSkillLogic = new IgnoreRestriction();
	            break;
			case "엄폐물":
			passiveSkillLogic = new S181_2_SkillLogic();
			break;
			case "마력 맛이 난다":
				passiveSkillLogic = new AttachStatusEffectToDestroyer ();
				break;
			case "휘몰아치는 열기":
				passiveSkillLogic = new FireSpiritPassive();
				break;
			case "폭발성":
				passiveSkillLogic = new ExplosiveSkillLogic();
				break;

			// 그레네브
			case "고지대 점령":
            passiveSkillLogic = new Grenev_P0_SkillLogic();
            break;
            case "살의":
            passiveSkillLogic = new Grenev_1_r_SkillLogic();
            break;
            case "살육의 희열":
            passiveSkillLogic = new Grenev_2_r_SkillLogic();
            break;
			case "원거리 사격":
				passiveSkillLogic = new Grenev_A15_SkillLogic();
				break;
            case "약자멸시":
            passiveSkillLogic = new Grenev_3_r_SkillLogic();
            break;
            case "전망":
            passiveSkillLogic = new Grenev_5_l_SkillLogic();
            break;
            case "암살":
            passiveSkillLogic = new Grenev_6_m_SkillLogic();
            break;
            case "전쟁광":
            passiveSkillLogic = new Grenev_6_r_SkillLogic();
            break;
            case "인내심":
            passiveSkillLogic = new Grenev_7_m_SkillLogic();
            break;
            case "연쇄살인":
            passiveSkillLogic = new Grenev_7_r_SkillLogic();
            break;

            //노엘
            case "심판의 빛":
            passiveSkillLogic = new Noel_0_l_SkillLogic();
            break;
            case "승리의 함성":
            passiveSkillLogic = new Noel_3_m_SkillLogic();
            break;
            case "주교의 권능":
            passiveSkillLogic = new Noel_3_r_SkillLogic();
            break;

            // 달케니르
            case "흐름 동화":
            passiveSkillLogic = new Darkenir_Unique_SkillLogic();
            break;
			case "공허의 장벽":
            passiveSkillLogic = new Darkenir_1_l_SkillLogic();
            break;
			case "흐름 분쇄":
				passiveSkillLogic = new Darkenir_A8_SkillLogic();
				break;
            case "허상의 이면":
            passiveSkillLogic = new Darkenir_2_m_SkillLogic();
            break;
            case "공허의 순환":
            passiveSkillLogic = new Darkenir_3_m_SkillLogic();
            break;
            case "무심한 주시":
            passiveSkillLogic = new Darkenir_3_r_SkillLogic();
            break;

			// 데우스
			case "약화강화":
				passiveSkillLogic = new Deus_0_1_SkillLogic ();
				break;
			case "노화":
			passiveSkillLogic = new Deus_2_r_SkillLogic();
			break;

			// 레이나
			case "핀토스의 긍지":
			passiveSkillLogic = new Reina_P0_SkillLogic();
			break;
			case "상처 태우기":
			passiveSkillLogic = new Reina_2_l_SkillLogic();
			break;
			case "불의 파동":
			passiveSkillLogic = new Reina_2_m_SkillLogic();
			break;
			case "점화":
			passiveSkillLogic = new Reina_3_l_SkillLogic();
			break;
			case "잉걸불":
			passiveSkillLogic = new Reina_3_m_SkillLogic();
			break;
			case "뭉치면 죽는다":
			passiveSkillLogic = new Reina_5_l_SkillLogic();
			break;
			case "흩어져도 죽는다":
			passiveSkillLogic = new Reina_5_m_SkillLogic();
			break;
			// case "잿더미":
			// passiveSkillLogic = new Reina_6_l_SkillLogic();
			// break;
			// case "흐름 변환":
			// passiveSkillLogic = new Reina_6_r_SkillLogic();
			// break;
			case "열상 낙인":
			passiveSkillLogic = new Reina_7_m_SkillLogic();
			break;
            // case "에테르 순환":
            // passiveSkillLogic = new Reina_7_r_SkillLogic();
            // break;

            // 루베리카
            case "셈여림":
            passiveSkillLogic = new Luvericha_Unique_SkillLogic();
            break;
            case "위기상황":
            passiveSkillLogic = new Luvericha_B8_SkillLogic();
            break;
            case "생명의 고동":
            passiveSkillLogic = new Luvericha_2_r_SkillLogic();
            break;
            case "치유의 오오라":
            passiveSkillLogic = new Luvericha_3_r_SkillLogic();
            break;
            case "넘치는 사랑":
            passiveSkillLogic = new Luvericha_5_r_SkillLogic();
            break;
            case "영혼의 교감":
            passiveSkillLogic = new Luvericha_Soul_SkillLogic();
            break;
            case "헌신":
            passiveSkillLogic = new Luvericha_7_m_SkillLogic();
            break;
            case "응급처치":
            passiveSkillLogic = new Luvericha_7_r_SkillLogic();
            break;

            //루키어스
            case "기사도":
            passiveSkillLogic = new Lucius_Unique_SkillLogic();
            break;
            case "배수진":
            passiveSkillLogic = new Lucius_B10_SkillLogic();
            break;
			case "검사의 직감":
				passiveSkillLogic = new Lucius_Intuition_SkillLogic();
				break;
			case "푸른 날개":
				passiveSkillLogic = new Lucius_C17_SkillLogic();
				break;
            case "정면 승부":
            	passiveSkillLogic = new Lucius_A20_SkillLogic();
            	break;

            // 리니안
            case "감전":
			passiveSkillLogic = new Lenien_Unique_SkillLogic();
			break;
			case "에너지 변환":
			passiveSkillLogic = new Lenien_B8_SkillLogic();
			break;
			case "전도체":
			passiveSkillLogic = new Lenien_2_r_SkillLogic();
			break;
			case "피뢰침":
			passiveSkillLogic = new Lenien_Rod_SkillLogic();
			break;
			case "정전기 유도":
			passiveSkillLogic = new Lenien_Static_SkillLogic();
			break;
			case "연쇄 방전":
			passiveSkillLogic = new Lenien_5_m_SkillLogic();
			break;
			case "자기 부상":
			passiveSkillLogic = new Lenien_5_r_SkillLogic();
			break;
			case "에테르 전위":
			passiveSkillLogic = new Lenien_6_l_SkillLogic();
			break;

            //비앙카
            case "사뿐사뿐":
            passiveSkillLogic = new Bianca_Unique_SkillLogic();
            break;
            case "의욕 상승":
            passiveSkillLogic = new Bianca_2_r_SkillLogic();
            break;
            case "관람석 선점":
            passiveSkillLogic = new Bianca_3_l_SkillLogic();
            break;
            case "응원 고마워!":
            passiveSkillLogic = new Bianca_3_r_SkillLogic();
            break;

            //세피아
            case "통제 구역":
            passiveSkillLogic = new Sepia_Unique_SkillLogic();
            break;
            case "신뢰의 끈":
            passiveSkillLogic = new Sepia_1_m_SkillLogic();
            break;
            case "정확한 피아식별":
            passiveSkillLogic = new Sepia_2_m_SkillLogic();
            break;
            case "정찰 복귀":
            passiveSkillLogic = new Sepia_A15_SkillLogic();
            break;

            //아르카디아
            case "뿌리 내리기":
            passiveSkillLogic = new ApplyOnPlantTile();
            break;
            case "광합성":
            passiveSkillLogic = new ApplyOnPlantTile();
            break;

			// 영
			case "방랑자":
			passiveSkillLogic = new Yeong_0_1_SkillLogic();
			break;
			case "어려운 목표물":
			passiveSkillLogic = new Yeong_HardTarget_SkillLogic();
			break;
			case "동체 시력":
			passiveSkillLogic = new Yeong_1_r_SkillLogic();
			break;
			case "간파":
			passiveSkillLogic = new Yeong_2_m_SkillLogic();
			break;
			case "기척 감지":
			passiveSkillLogic = new Yeong_2_r_SkillLogic();
			break;
			case "질풍노도":
			passiveSkillLogic = new Yeong_3_m_SkillLogic();
			break;
			case "위험 돌파":
			passiveSkillLogic = new AttachOnStart();
			break;
			case "유법":
			passiveSkillLogic = new Yeong_5_m_SkillLogic();
			break;
			// case "접근":
			// passiveSkillLogic = new Yeong_6_l_SkillLogic();
			// break;
			case "무아지경":
			passiveSkillLogic = new Yeong_6_m_SkillLogic();
			break;
			case "끝없는 단련":
			passiveSkillLogic = new Yeong_6_r_SkillLogic();
			break;
			// case "후의 선":
			// passiveSkillLogic = new Yeong_7_m_SkillLogic();
			// break;
			case "명경지수":
			passiveSkillLogic = new Yeong_7_r_SkillLogic();
			break;

			// 에렌
			case "에테르 지배":
			passiveSkillLogic = new Eren_0_1_SkillLogic();
			break;
			case "흑화":
			passiveSkillLogic = new Eren_2_l_SkillLogic();
			break;
			case "축성":
			passiveSkillLogic = new Eren_2_r_SkillLogic();
			break;
			case "초월":
			passiveSkillLogic = new Eren_3_m_SkillLogic();
			break;
			case "배척받는 자":
			passiveSkillLogic = new Eren_5_l_SkillLogic();
			break;
			case "영겁의 지식":
			passiveSkillLogic = new Eren_5_m_SkillLogic();
			break;
			case "진형 붕괴":
			passiveSkillLogic = new Eren_6_m_SkillLogic();
			break;
			case "압도적인 공포":
			passiveSkillLogic = new Eren_7_l_SkillLogic();
			break;
			case "천상의 전령":
			passiveSkillLogic = new Eren_7_l_SkillLogic();
			break;

				//유진
			case "거울 공명":
				passiveSkillLogic = new Eugene_P0_SkillLogic ();
				break;
			case "순은의 매듭":
				passiveSkillLogic = new Eugene_B8_SkillLogic();
				break;
			case "색다른 휴식":
            passiveSkillLogic = new Eugene_2_r_SkillLogic();
            break;
            case "여행자의 발걸음":
            passiveSkillLogic = new Eugene_3_r_SkillLogic();
            break;
            case "순수한 물":
            passiveSkillLogic = new Eugene_5_l_SkillLogic();
            break;
            case "은의 지평선":
            passiveSkillLogic = new Eugene_5_m_SkillLogic();
            break;
            case "야영 전문가":
            passiveSkillLogic = new Eugene_6_r_SkillLogic();
            break;
            case "청명수의 은총":
            passiveSkillLogic = new Eugene_7_l_SkillLogic();
            break;
            case "길잡이":
            passiveSkillLogic = new Eugene_7_r_SkillLogic();
            break;

			//라티스
			case "속죄양":
				passiveSkillLogic = new Ratice_Unique_SkillLogic ();
				break;
			case "수호의 오오라":
				passiveSkillLogic = new Ratice_1_l_SkillLogic ();
				break;
			case "흔들림 없는 눈빛":
				passiveSkillLogic = new Ratice_2_m_SkillLogic ();
				break;
			case "일치단결":
				passiveSkillLogic = new Ratice_Unite_SkillLogic ();
				break;
			case "전의 상실":
				passiveSkillLogic = new Ratice_3_r_SkillLogic ();
				break;

            //제이선
            case "집중 공격":
            passiveSkillLogic = new Json_Unique_SkillLogic();
            break;
            case "바람 가르기":
            passiveSkillLogic = new Json_WindDivision_SkillLogic();
            break;
            case "비틀린 칼날":
            passiveSkillLogic = new Json_2_m_SkillLogic();
            break;
			case "치고 빠지기":
			passiveSkillLogic = new Json_2_r_SkillLogic();
			break;
			case "연타":
			passiveSkillLogic = new Json_3_l_SkillLogic();
			break;

            // 트리아나
            case "들불":
                passiveSkillLogic = new Triana_WildFire_SkillLogic();
            	break;

            // 카샤스티
            case "전술 지원":
            passiveSkillLogic = new Kashasty_Unique_SkillLogic();
            break;
			case "장미 속의 가시":
				passiveSkillLogic = new Kashasty_C1_SkillLogic();
				break;
			case "지원 사격":
				passiveSkillLogic = new Kashasty_B8_SkillLogic();
				break;
			case "시들지 않는 의지":
				passiveSkillLogic = new AttachOnStart();
				break;
            case "그물탄":
            passiveSkillLogic = new Kashasty_3_l_SkillLogic();
            break;
            case "마법공학 스캐너":
            passiveSkillLogic = new Kashasty_Scanner_SkillLogic();
            break;
			case "장미의 사수":
				passiveSkillLogic = new Kashasty_C36_SkillLogic();
				break;

            //칼드리치
            case "모두에게 평등한 죽창":
            passiveSkillLogic = new Karldrich_0_1_SkillLogic();
            break;
            case "행동하는 혁명가":
            passiveSkillLogic = new Karldrich_1_r_SkillLogic();
            break;
            case "노조결성":
            passiveSkillLogic = new Karldrich_2_r_SkillLogic();
            break;
            case "좌편향":
            passiveSkillLogic = new Karldrich_LeftWing_SkillLogic();
            break;
            case "강자멸시":
            passiveSkillLogic = new Karldrich_3_m_SkillLogic();
            break;

            // 큐리
            case "정제":
            passiveSkillLogic = new Curi_0_1_SkillLogic();
            break;
            case "호기심":
            passiveSkillLogic = new Curi_1_1_SkillLogic();
            break;
            case "신속 반응":
            passiveSkillLogic = new Curi_2_1_SkillLogic();
            break;
            case "가연성 부착물":
            passiveSkillLogic = new Curi_B8_SkillLogic();
            break;
            case "조연성 부착물":
            passiveSkillLogic = new Curi_B15_SkillLogic();
            break;
            case "동적 평형":
            passiveSkillLogic = new Curi_3_1_SkillLogic();
            break;
            case "환원":
            passiveSkillLogic = new Curi_3_r_SkillLogic();
            break;
            case "재결정":
            passiveSkillLogic = new Curi_5_l_SkillLogic();
            break;
            case "제한 구역":
            passiveSkillLogic = new Curi_7_l_SkillLogic();
            break;
            default:
			passiveSkillLogic = new BasePassiveSkillLogic();
            break;
           
		}

		passiveSkillLogic.passiveSkill = passiveSkill;
		passiveSkillLogic.skill = passiveSkill;

		return passiveSkillLogic;
	}
}
}

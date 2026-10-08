# GPS — сумісність установленої зброї

Знімок поточної установки Nuclear Option від 2026-10-08 після завантаження Blueprinter. Тут наведено бомби, ракети та касетні/внутрішні боєприпаси. Гармати й лазерні установки вилучено як такі, для яких ця система GPS не призначена. UFO-записи, зовнішні паливні баки, вантажі, радарні/службові контейнери, транспорт і засоби протидії також вилучено. Однакові назви з різними asset ID — різні визначення.

«Так» означає сумісність поточного GPS-адаптера з компонентами префаба, а не підтверджений польотний тест кожного типу. GPS-запуск доступний у single player або хосту. Лазерним бомбам потрібне підсвічування для лазерного кінцевого наведення; керованим касетам — реальна видима ворожа ціль біля мітки для розкриття.

| Зброя | Asset ID | GPS | Компонент / умова |
| --- | --- | --- | --- |
| 3M22 Zircon | zircon info | Так | OpticalSeeker |
| 3N22 Zircon 250kt | zircon nuke info | Так | OpticalSeeker |
| 3S-RMM | 1509_AAM1Gelb_info | Ні | IRSeeker; адаптер не підтримує це визначення |
| AAM-120C | MeridianAMRAAM_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-29 Scythe | AAM2_info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-36 Scimitar | AAM4_info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-41 Gram | MeridianAAM41_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-45 Sabre | Aryx_Missile_AAM45_Info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-54 Phoenix | MeridianAAM54_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-63 Falchion | MeridianAAM63_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AAM-90 Estoc | MeridianScreamer_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AGM-102 Kalibr | MeridianKalibr_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| AGM-102E Kalibr | MeridianKalibrEW_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| AGM-111 Nemesis | Aryx_FastAShM1_Info | Так | OpticalSeekerCruiseMissile |
| AGM-114A | BL_AGM_114_A_WI | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGM-114K | BL_AGM_114_K_WI | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGM-114L | BL_AGM_114_L_WI | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AGM-180 Blackout | WI_Blackout | Так | OpticalSeekerCruiseMissile |
| AGM-190A Black Arrow | MeridianBlackArrow_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| AGM-27 Pyre | Aryx_FastAGM1_Info | Так | OpticalSeeker |
| AGM-33L | MeridianAGM33L_WeaponInfo | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGM-48 | info_AGM1 | Так | OpticalSeeker |
| AGM-57L | MeridianAGM57L_WeaponInfo | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGM-68 | info_AGM_heavy | Так | OpticalSeeker |
| AGM-76 Atlatl | Aryx_SmallCruiseMissile1_info | Так | OpticalSeekerCruiseMissile |
| AGM-84 | MeridianAGM84_WeaponInfo | Так | OpticalSeeker |
| AGM-84H SLAM-ER | AGM84H_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| AGM-84K SLAM-ER | agm 84 info | Так | OpticalSeekerCruiseMissile |
| AGM-92 | MeridianAGM92_WeaponInfo | Так | OpticalSeeker |
| AGM-99 | AShM2_info | Так | OpticalSeekerCruiseMissile |
| AGR-18 Lynchpin | info_rocket1 | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGR-24 Kingpin | info_rocket2 | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGR-31 Tenpin | Tenpin_WeaponInfo | Ні | InertialSeekerShell; адаптер не підтримує це визначення |
| AGR-31 Tenpin | Tenpin_WeaponInfo18 | Ні | InertialSeekerShell; адаптер не підтримує це визначення |
| AGR-31 Tenpin | Tenpin_WeaponInfo19 | Ні | InertialSeekerShell; адаптер не підтримує це визначення |
| AGR-37 Jester | Aryx_HeavyRocket1_info | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AGR-40 Hairpin | MeridianAGR40_Missile_WeaponInfo | Ні | IRSeeker; адаптер не підтримує це визначення |
| AGR-51 Strike | Tenpin_WeaponInfo_Strike | Ні | InertialSeekerShell; адаптер не підтримує це визначення |
| AGR-75 Sledgepin | MissilePack_AGR75Sledgepin_Info | Ні | Лазерна ракета; потрібне штатне підсвічування |
| AIM-120D AMRAAM | Aim 120 info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AIM-160X SACM | cuda inf | Ні | ARHSeeker; адаптер не підтримує це визначення |
| AIM-2000 IRIS-T | IRIS-T Info | Ні | IRSeeker; адаптер не підтримує це визначення |
| AIM-9M Sidewinder | Aim 9m addon info | Ні | IRSeeker; адаптер не підтримує це визначення |
| AIM-9X Sidewinder | aim 9x info | Ні | IRSeeker; адаптер не підтримує це визначення |
| AIM-9Z Sidewinder II | aim 9z info | Ні | IRSeeker; адаптер не підтримує це визначення |
| AIR-2 Genie | info_AIR-2_Genie | Так | OpticalSeeker |
| ALM-C450 | info_CruiseMissile1 | Так | OpticalSeekerCruiseMissile |
| ALND-4 (20kt) | info_CruiseMissile20kt | Так | OpticalSeekerCruiseMissile |
| AM39E Exocet | Exocet info | Так | OpticalSeekerCruiseMissile |
| Apex-6 | WI_Apex6_PalletDrone | Так | OpticalSeekerCruiseMissile |
| Apex-6 Ground | WI_Apex6_Ground | Так | OpticalSeekerCruiseMissile |
| Apex-8 | WI_Apex6_Air | Так | OpticalSeekerCruiseMissile |
| Apex-8 | WI_Apex8_Ground | Так | OpticalSeekerCruiseMissile |
| ARAD-116 | ARM1_info | Ні | ARMSeeker; адаптер не підтримує це визначення |
| ARAD-120 | MeridianARAD200_WeaponInfo | Ні | ARMSeeker; адаптер не підтримує це визначення |
| ARAD-45 | ARM2_info | Ні | ARMSeeker; адаптер не підтримує це визначення |
| ARAD-45 | Aryx_Wep_ARM2_Info | Ні | ARMSeeker; адаптер не підтримує це визначення |
| ARAD-72 | MeridianARAD72_WeaponInfo | Ні | ARMSeeker; адаптер не підтримує це визначення |
| ASD-16 Submunition Dispenser | Aryx_Wep_BombletDispenser_Info | Ні | OpticalSeeker; адаптер не підтримує це визначення |
| AShM-140 Exocet | MeridianExocetAir_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| AShM-300 | info_AShM1 | Так | OpticalSeekerCruiseMissile |
| AShM-500 Yashma | MeridianYashma_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| AT-145 | GTG1_info | Так | OpticalSeeker |
| ATMACA-AL | atmaca info | Так | OpticalSeekerCruiseMissile |
| ATP-1 | info_AGM2 | Так | OpticalSeeker |
| CBO-400 | Aryx_ClusterBomb1_Info | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| CBO-400 | Aryx_LightFighter1_ClusterBomb1_Info | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| CBO-400 | info_Bomb_cluster1 | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| CBU-82M Locust | WI_Locust | Ні | Некерована мінна касета; CCIP |
| CBU-82S Lawn Chair | WI_LawnChair | Ні | Некерована мінна касета; CCIP |
| Demolition Bomb | info_bomb_demolition | Так | OpticalSeekerBomb |
| Eyeball Mk.II | info_AGM_scanner1 | Так | OpticalSeeker |
| GBM-500LR | info_bomb_500_glide | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| GBO-900 | MeridianGBO900_WeaponInfo | Так | OpticalSeeker |
| GBP-500 Bodkin | MeridianGBP500_WeaponInfo | Так | OpticalSeekerBomb |
| GBU-38/B JDAM (500lb) | dani_largebomb_500_info | Так | OpticalSeekerBomb |
| GPM-550X | MissilePack_GPM550X_Info | Так | OpticalSeeker |
| GPO-2P Auger | info_bomb_penetrator1 | Так | OpticalSeekerBomb |
| GPO-500 | info_blastFrag500 | Так | OpticalSeekerBomb |
| GPO-N (1.5kt) | info_nuclearBomb1 | Так | OpticalSeekerBomb |
| GPO-N (250kt) | info_nuclearBomb1_strategic | Так | OpticalSeekerBomb |
| GS25 | Submunition1_info | Ні | OpticalSeeker; адаптер не підтримує це визначення |
| HSM-160 Kickback | MeridianHSM160_WeaponInfo | Так | BallisticMissileGuidance |
| HSM-160C Scatter | MeridianHSM160C_WeaponInfo | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| HSM-160M Hailstorm (20kt) | MeridianHSM160M_WeaponInfo | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| HSM-160N Longshot (300kt) | MeridianHSM160N_WeaponInfo | Так | BallisticMissileGuidance |
| HSM-290 "Killjoy" (HE) | WI_Kinzhal_HE | Так | BallisticMissileGuidance |
| HSM-290 "Killjoy" (Nuclear) | WI_Kinzhal_Nuclear | Так | BallisticMissileGuidance |
| HSM-750 Starfall | Aryx_Hypersonic1_Info | Ні | AryxInertialSeeker; адаптер не підтримує це визначення |
| HSM-N Sunfall | Aryx_Hypersonic1_Nuke_Info | Ні | AryxInertialSeeker; адаптер не підтримує це визначення |
| HT-200 'Hammerhead' | TorpedoBig_info | Так | OpticalSeekerCruiseMissile |
| Hydra-70 [M247] | BL_H70_M247_WI | Ні | InertialSeekerShell; адаптер не підтримує це визначення |
| Hydra-70 APKWS [M247] | BL_H70_M247_APKWS_WI | Так | LaserSeeker |
| IRM-L7 | MeridianIRML7_WeaponInfo | Ні | IRSeeker; адаптер не підтримує це визначення |
| IRM-S1 | info_SAM_IR1 | Ні | IRSeeker; адаптер не підтримує це визначення |
| IRM-S2 | AAM3_info | Ні | IRSeeker; адаптер не підтримує це визначення |
| IRM-S5 Gladius | Aryx_SRIRM1_info | Ні | IRSeeker; адаптер не підтримує це визначення |
| K-30M | k30 info | Ні | IRSeeker; адаптер не підтримує це визначення |
| Locust Mine | WI_LocustMine | Ні | Немає підтримуваного керованого наведення |
| MBDA Brimstone | Brimstone Info | Так | OpticalSeeker |
| MBDA METEOR | METEOR info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| MK-88 Hydra | MissilePack_MK54_Info | Так | OpticalSeeker |
| MLRS Rocket | info_rocket_MLRS1 | Ні | InertialSeekerShell; адаптер не підтримує це визначення |
| MMR-S3 | AAM1_info | Ні | IRSeeker; адаптер не підтримує це визначення |
| MRM-S4 Broadsword | Aryx_LRIRM1_info | Ні | IRSeeker; адаптер не підтримує це визначення |
| MW-220 | k_MW_info | Так | OpticalSeeker |
| NAAM-432 Caldera | NAAM_1_weapon_info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| NL-98 | ARH1_info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| NT-2 'Megalodon' (20kt) | Torpedo20kt_info | Так | OpticalSeekerCruiseMissile |
| PAB-125 | info_bomb_125_1 | Так | OpticalSeekerBomb |
| PAB-125HD | info_bomb_125_HD | Так | OpticalSeekerBomb |
| PAB-250 | info_bomb_250_1 | Так | OpticalSeekerBomb |
| PAB-250LR | info_bomb_250_glide | Так | OpticalSeeker |
| PAB-3000LR | Aryx_Wep_GlideBomb_3000_info | Так | OpticalSeeker |
| PAB-80LR | info_bomb_glide1 | Так | OpticalSeeker |
| PGM-400LR | Aryx_GlideBomb400_Info | Так | OpticalSeeker |
| Piledriver QBM (MWC) | k_TBM_MWC_info | Так | Керована касета: потрібна реальна видима ціль біля мітки |
| Piledriver TBM | ballisticMissile1_info | Так | BallisticMissileGuidance |
| Piledriver TBM (20kt) | ballisticMissile1_tacNuke_info | Так | BallisticMissileGuidance |
| PL-15E | PL15_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| PL-17 | PL17_WeaponInfo | Ні | ARHSeeker; адаптер не підтримує це визначення |
| R-100N Zenith | Aryx_HASAM1_info | Ні | AryxABMSeeker; адаптер не підтримує це визначення |
| R-27ER | R27 info | Ні | SARHSeeker; адаптер не підтримує це визначення |
| R-460 Poseidon | WI_R460_Poseidon | Так | OpticalSeekerCruiseMissile |
| R-460 Poseidon | WI_R460_Poseidon_TEL | Так | OpticalSeekerCruiseMissile |
| R-460N Poseidon (1.5 kt) | WI_R460_Poseidon_Nuclear | Так | OpticalSeekerCruiseMissile |
| RAM-45 | info_SAM_Radar1 | Ні | SARHSeeker; адаптер не підтримує це визначення |
| RGM-68 | 1509_Info_ShipAGM | Так | OpticalSeeker |
| SAM-85 Whirlwind | Aryx_Supercarrier1_SAM_R85_Info | Ні | SARHSeeker; адаптер не підтримує це визначення |
| SCT-350 'Mako' | TorpedoFast_info | Так | OpticalSeekerCruiseMissile |
| SD-6 Needle | MeridianSD6Needle_Encyclopedia_info | Ні | OpticalSeeker; адаптер не підтримує це визначення |
| SD-6N Needle | MeridianSD6NNeedle_Encyclopedia_info | Ні | OpticalSeeker; адаптер не підтримує це визначення |
| Shahed-136 | Shahed136_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| SLND-9 (20kt) | Aryx_Navex_SLND_Info | Так | OpticalSeekerCruiseMissile |
| SRM-20 Merlin | MeridianSRM20_WeaponInfo | Ні | IRSeeker; адаптер не підтримує це визначення |
| SRM-8 Kukri | MeridianSRM8_WeaponInfo | Ні | IRSeeker; адаптер не підтримує це визначення |
| StormX Shadow | storm info | Так | OpticalSeekerCruiseMissile |
| StratoLance R9 | info_SAM_Radar2 | Ні | SARHSeeker; адаптер не підтримує це визначення |
| Tusko-B (HE) | AShM3_info | Так | OpticalSeeker |
| Tusko-N (1.5kt) | Aryx_Wep_TuskoN_info | Так | OpticalSeeker |
| Type-88 'Lemon' | TorpedoLight_info | Так | OpticalSeekerCruiseMissile |
| X-77 Warewind | MissilePack_X77_Info | Ні | ARHSeeker; адаптер не підтримує це визначення |
| YJ-18E | YJ18_WeaponInfo | Так | OpticalSeekerCruiseMissile |
| YJ-20 ASBM | YJ20_WeaponInfo | Ні | YJ20Seeker; адаптер не підтримує це визначення |
| Zhdan Sensor Mine | WI_ZhdanMine | Ні | Немає підтримуваного керованого наведення |

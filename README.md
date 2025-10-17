>[!WARNING]
>This repository is under development!

# OWASP Tool: CureCode
Context-aware web-based tool that aims to support healthcare developers and testers in applying OWASP standards thorugh the lifecycle (design, development, and testing) of clinical web applications. The tool aims to make OWASP guidelines more interactive, usable, and tailored to the specific characteristics of the application being developed or tested.

The tool architecture is developed around three main modules, corresponding to distinct phases of the software lifecycle: <b>contextual profiling</b>, <b>secure development</b>, <b>security testing</b>. These modules are designed to work in sequence, but each can also function independently depending on the user's needs.

## 1<sup>st</sup> Module: Contextual Profiling
The first Module is designed to assist healthcare developers and testers of healthcare web applications in determining the appropriate security level required for their application, based on its specific context. The module is composed of multiple steps, each aimed at collecting and evaluating key information to build an accurate risk profile and select relevant ASVS and WSTG requirements accordingly:
* <b>1<sup>st</sup> Step: Security Level Classification.</b> The goal is to classify healthcare web applications according to their required security level and contextual risk. To support this classification, a workflow inspired by NIST SP 800-63 assurance levels was designed and adapted to the clinical context. This workflow guides the user through a structured set of questions, each contributing to the definition of a risk profile based on healthcare-specific harm and impact categories. The resulting profile determines the initial security classification of the application. For more information on selecting the security level, see Informative Section: Security Level Section.
* <b>2<sup>nd</sup> Step: Application Characterization.</b> The second step of the module focuses on gathering a more detailed understanding of the application itself, in order to refine the risk profile and contextualize the security requirements further. While Step 1 provides a general classification based on abstract risk factors, Step 2 analyzes the concrete characteristics of the application. The information collected in this step, through a set of structured questions, allows the tool to enable the selection of the most relevant ASVS requirements and WSTG test cases tailored to the specific features of the application.

## 2<sup>nd</sup> Module: Secure Development
In the second Module, the tool presents a sets of ASVS requirements, depending of the first module, organized by categories. For each requirement, users will be able to indicate whether it has been implemented, add technical notes, and explore links to relevant CWE entries. The outcome is a custom checklist of applicable and actionable security controls, which can be exported and used as part of the development documentation or security audits.

## 3<sup>rd</sup> Module: Security Testing
The third Module is dedicated to the testing phase, leveraging OWASP WSTG. Based on the application profile defined in the first Module, the tool suggests appropriate tests, each accompanied by a technical description, references to relevant CWE entries, and tools that can be used to perform the tests. The user can record whether the test was executed and passed, annotate technical observations, and generate a structured testing report.

>[!NOTE]
>This tool does not aim to replace the OWASP documentation or frameworks, but to act as a bridge between theory and practice. By translating standards into context-sensitive and actionable tasks, it enables even non-specialist teams to align with security best practices.

## Informative Section 1: Security Level Selection
According to the OWASP ASVS, web applications can be categorized in three security verification levels:
- <b>ASVS Level 1:</b> For low assurance levels, completely penetration testable.
- <b>ASVS Level 2:</b> Applications that contain sensitive data, which requires protection and is the recommended level for most apps.
- <b>ASVS Level 3:</b> Most critical applications - applications that perform high value transactions, contain sensitive medical data, or any application that requires the highest level of trust.

| Security Level | Healthcare web app type | Rationale |
| -------------- | ----------------------- | --------- |
| ASVS Level 1   | Health education portal with no login, publishing articles and infographics on healthy lifestyle (no user data collected, only technical cookies). | Reduced attack surface: no accounts, no uploads, no sensitive APIs. An attack would bring more of a reputational damage, but zero impact on data or patient care. |
| ASVS Level 2   | Medical appointment booking portal that manages patient profiles with demographics information and agenda, accessible via username + password and CAPTCHA; includes REST API to synchronize with the outpatient clinic’s queue management system. | It exposes PII and integrates APIs. However, it doesn’t contain detailed clinical data or functionality that directly affects diagnosis/therapy. |
| ASVS Level 3   | Electronic prescribing and drug therapy system used on the ward, with Single Sign-On authentication and digital signature, which updates the medical record in real time and sends orders to smart infusion pumps. It allows PDF report uploads and manages complete clinical data. | Multiple attack surfaces. Any compromise can alter therapies or expose PHI. More weaknesses, thus high probability of accident if advanced controls are not implemented rigorously. |

With this in mind, the <b>1<sup>st</sup> Step</b> of the <b>1<sup>st</sup> Module</b> focus on defining the appropriate security level category of the healthcare web application. I propose a security level framework inspired by NIST 800-63. I developed a workflow that identifies specific risks relevant to the healthcare environment. Based on the severity of these risks, the healthcare web application can be classified into one of three security levels: low, medium, or high. These three security levels correspond to ASVS L1, L2, and L3. 

In particular, to evaluate the level of security, the following healthcare-specific harm and impact categories will be considered:
| Code | Risk category | Description |
| ---- | ------------- | ----------- |
| H1 | Health data integrity compromise | Alteration or corruption of clinical data leading to inappropriate care or loss of reliable historicity. |
| H2a | Demographic data breach (PII) | Exposure of PII (both patients and doctors or other actor), without clinical content. |
| H2b | Identifiable clinical information breach (PHI) | Leakage of medical records, reports, diagnostic images or therapies associated directly with an identified patient. Involves potential stigma, reputational damage, and healthcare legal liability. |
| H3 | Healthcare financial fraud or abuse | Theft of billing credentials. |
| H4 | Other clinical delays or errors | Any impact that slows, prevents, or alters diagnostic/therapeutic decisions. |
| H5 | Reputational/legal damage to healthcare organization | Loss of trust, GDPR/NIS2 penalties. |
| H6 | Disruption of clinical services | Prolonged unavailability of systems that support direct patient care. |
| H7 | Risk to patient physical safety  | Situations in which access to or alteration of data can cause injury or death. |
| H8 | Invalidation of trial data |  |
| H9 | Re-identification and possible of sensitive information from pseudonymized data | An attacker (or third party) links pseudonymized datasets to external information, recovering patients' identities. |

### Step 1.1: High level risk categories evaluation
The first step involves assessing the highest-level risk categories, which would directly affect patient or healthcare personnel, data confidentiality, or healthcare information. These categories are:
- <b>H1:</b>  <i>Health data integrity compromise.</i> This category refers to any unauthorized alteration, corruption, or deletion of clinical data. Such incidents may cause misdiagnosis, inappropriate treatments, or the loss of historically important clinical information .
- <b>H2a:</b> <i>Demographic data breach (PII).</i> Exposure of Personally Identifiable Information (PII) such as names, birthdates, or contact details (whether related to patients, doctors, or staff) is a significant privacy concern.
- <b>H2b:</b> <i>Identifiable  clinical information breach (PHI).</i> This category involves unauthorized access to or dissemination of identifiable medical data, including lab results, diagnostic images, prescriptions, or detailed clinical reports. Such breaches not only expose patients to potential stigma and discrimination but also result in legal consequences and lasting damage to institutional trust.
- <b>H3:</b> <i>Healthcare financial fraud or abuse.</i> Attacks that compromise billing systems, payment authorizations, or insurance data fall into this category. Misuse of billing credentials can result in financial loss, fraudulent charges, or regulatory penalties.
- <b>H4:</b> <i>Other clinical delays or errors.</i> Any other impact that slows, prevents, or alters diagnostic/therapeutic decisions.

### Step 1.2: Decision logic
If at least one of these critical categories is assessed as high risk, the application is automatically classified as L3. If none of these categories are high, but either H2a or H2b is considered moderate, this still justifies a L3 classification, given the legal sensitivity of PII or PHI exposure. If H2a and H2b are not rated high or moderate, but H1, H3 or H4 are evaluated as moderate, the application is classified at L2. If all the categories in this group are considered low or none, proceed to Step 2.1.

The following image represents the decisional workflow of the <b>first step</b>. In the website the buttons are interactive to facilitate the <i>interaction</i> with the schema.

![STEP 1](/OwaspTool/wwwroot/img/step1.jpg)

### Step 2.1: Low level risk categories evaluation
If none of the critical risks reach Moderate or High levels, the assessment continues with categories which can be direct or indirect consequences of the high level risk categories.
- <b>H5:</b> <i>Reputational and legal damage to the healthcare organization.</i> A breach or incident may lead to loss of patient and public trust, and potential violations of laws such as GDPR or NIS2. These legal consequences may result in significant financial penalties or operational restrictions.
- <b>H6:</b> <i>Disruption of clinical services.</i> This refers to system downtime or malfunction that impedes care delivery, e.g., a radiology system becoming unavailable, or electronic prescriptions failing. Prolonged disruptions can directly affect patient care continuity.
- <b>H7:</b> <i>Risk to patient physical safety.</i> This risk occurs when data tempering or system failure leads to clinical actions that may result in injury or even death.
- <b>H8:</b> <i>Invalidation of trial data.</i> for systems involved in clinical trials, any breach that alters or exposes research data may invalidate scientific works. This not only affects medical progress but can undermine compliance with trial protocols and regulatory approvals.
- <b>H9:</b> <i>Re-identification and possible spread of sensitive information from pseudonymized data.</i> When data is pseudonymized, there’s a risk that attackers may cross-reference it with external datasets to reconstruct patient identities. This kind of breach is subtle but deeply invasive, undermining patient confidentiality.

### Step 2.2: Decision Logic
If H7 or H9 are evaluated as high or moderate, then the system is classified as L3. If none of these categories are rated, above Low, the system may be safely classified as L1, which assumes only minimal residual risk.

The following image represents the decisional workflow of the <b>second step</b>. In the website the buttons are interactive to facilitate the <i>interaction</i> with the schema.

![STEP 2](/OwaspTool/wwwroot/img/step2.jpg)
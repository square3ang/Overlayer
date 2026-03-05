using static Overlayer.Olly.OllyDialogue;
using static Overlayer.Olly.OllyState;
using static Overlayer.Olly.OllyUtils;

namespace Overlayer.Olly;

public partial class Olly {
    private Node MakeDialogue() {
        var node1 = new Node(
            "...",
            new[] { Tr("About you", "너에 관해"), Tr("Overlayer", "오버레이어"), Tr("Bugs", "버그"), Tr("About Talk", "대화에 관해"), Tr("Face", "얼굴") },
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.Normal,
            onChoice: (num) => {
                if(num == 4) {
                    FollowMouse = true;
                }
            }
        );

        var faceNode = new Node(
            "..",
            new[] { Tr("Back", "뒤로가기") },
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.Normal,
            onChoice: _ => {
                unsafe {
                    FollowMouse = false;
                }
            }
        );
        faceNode.Next[0] = node1;

        var node2 = new Node(
            Tr("I'm Olly.\nThe name comes from words used in Overlayer.", "저는 올리에요.\nOverlayer에 들어간 단어로부터 이름을 지었다고 하네요."),
            new[] { "...", Tr("How did you come to be?", "어쩌다가 만들어졌어?"), Tr("I like your name.", "이름이 좋네."), Tr("Who drew you?", "널 누가 그려준거야?") },
            eye: Eye.Normal,
            mouth: Mouth.OpenSmall,
            eyebrow: Eyebrow.Normal
        );

        var node2_2 = new Node(
            Tr("They just added some interesting things they thought of along the way.", "적당히 넣을만한..\n신기한 것들을 생각하다가 넣으셨다고 전하시라네요."),
            new[] { "..." },
            eye: Eye.Normal,
            mouth: Mouth.Clenched,
            eyebrow: Eyebrow.NormalHigh
        );

        var node2_3 = new Node(
            Tr("Hmm.. thank you for the compliment.\nHearing that gives me a bit of motivation.", "으음.. 칭찬 고마워요.\n그렇게 말해주니 힘이 나네요."),
            new[] { "..." },
            eye: Eye.Small,
            mouth: Mouth.Caret,
            eyebrow: Eyebrow.NormalHigh
        );

        var node2_4 = new Node(
            Tr("Kkitut drew me.\nThat’s how I ended up looking like this.", "Kkitut님이 저를 그려주셨어요.\n덕분에 이렇게 생기게 되었네요"),
            new[] { "...", Tr("ugly", "못생겼는데"), Tr("You’re kinda cute.", "조금 귀여운 걸") },
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.Normal
        );

        var node2_4_2 = new Node(
            Tr("Darn..", "이런.."),
            new[] { "..." },
            eye: Eye.Small,
            mouth: Mouth.Shift,
            eyebrow: Eyebrow.Sad,
            effectBit: EffectBit.Tear
        );

        var node2_4_3 = new Node(
            "//",
            new[] { Tr("Back", "뒤로가기") },
            eye: Eye.Small,
            mouth: Mouth.Caret,
            eyebrow: Eyebrow.NormalHigh,
            effectForwardBit: EffectForwardBit.BlushBig
        );

        var node3 = new Node(
            Tr("Originally it was a mod made by c3nb,\nbut now mostly Kkitut is in charge.\nThey had a hard time rewriting c3nb's code.", "처음엔 c3nb가 만든 모드였지만,\n이제는 대부분 Kkitut이 맞고 있어요.\nc3nb의 코드를 갈아치우느라 고생이 많았다고 했죠."),
            new[] { "...", Tr("Hard time?", "고생?") },
            eye: Eye.Normal,
            mouth: Mouth.Open,
            eyebrow: Eyebrow.Normal
        );

        var node3_2 = new Node(
            Tr("You may not know this, but the previous Overlayer was full of ads...\nIt was quite inconvenient in many ways.", "아실 진 모르겠지만..\n이전 오버레이어는 광고 투성이에...\n여러모로 불편하기 짝이 없었거든요."),
            new[] { "...", Tr("So..?", "그래서..?") },
            eye: Eye.Small,
            mouth: Mouth.OpenDisgust,
            eyebrow: Eyebrow.Sad
        );

        var node3_2_2 = new Node(
            Tr("So they spent quite a lot of time rewriting it from scratch.", "그래서 그걸 갈아엎느라 좀 시간을 많이 쓰셨다고 한다네요."),
            new[] { Tr("Back", "뒤로가기") },
            eye: Eye.Normal,
            mouth: Mouth.Shift,
            eyebrow: Eyebrow.NormalHigh
        );

        var node4 = new Node(
            Tr("Overlayer isn’t perfect.\nNeither am I.", "오버레이어는 완벽하지 않아요.\n저도 그렇고요."),
            new[] { Tr("Why you?", "너는 왜?"), Tr("Everyone has flaws", "누구나 결점은 있잖아") },
            eye: Eye.Normal,
            mouth: Mouth.Open,
            eyebrow: Eyebrow.NormalHigh
        );

        var node4_1 = new Node(
            Tr("Think about it...\nI’m just a piece of code inside Overlayer.", "한번 생각해봐요..\n저도 사실은 오버레이어의 일부분인 코드이잖아요."),
            new[] { Tr("So what?", "그게 뭐?"), Tr("..True", "..그렇네") },
            eye: Eye.Small,
            mouth: Mouth.Caret,
            eyebrow: Eyebrow.Normal
        );

        var node4_2 = new Node(
            Tr("...Indeed.\nSometimes flaws let us recognize and understand each other.",
               "...그러네요.\n결점 덕분에 우리가 서로를 알아보고 이해할 수도 있는 거겠죠."),
            new[] { "...", Tr("If possible, help each other along the way.", "가능하다면, 도우면서 살아야지.") },
            eye: Eye.Normal,
            mouth: Mouth.Clenched,
            eyebrow: Eyebrow.NormalHigh
        );

        var node4_1_1 = new Node(
           Tr("Hmm.. It's nothing special.\nNo need to worry about it.",
              "음.. 뭐 별거 아니에요.\n신경쓰지 않아도 괜찮아요."),
           new[] { Tr("Back", "뒤로가기") },
           eye: Eye.Normal,
           mouth: Mouth.SurpriseSmall,
           eyebrow: Eyebrow.NormalHigh
        );

        var node4_1_2 = new Node(
           Tr("Even with bugs and limits,\nI sometimes wonder if a being like me can have meaning because of this structure.",
              "버그나 한계가 있더라도,\n이런 구조 덕분에 저 같은 존재가 의미를 가질 수 있을까 하고 생각하기도 해요."),
           new[] { Tr("I see.", "그렇구만.") },
           eye: Eye.Normal,
           mouth: Mouth.OpenSmallHarf,
           eyebrow: Eyebrow.Normal
        );

        var node4_2_2 = new Node(
            Tr("That's right.\nIt's important to live helping each other.\nOverlayer follows a similar idea, released under GNU v3.",
               "그렇죠,\n서로 도움을 주고받으면서 살아가는 게 중요해요.\n오버레이어도 비슷한 생각으로 오픈소스인 GNU v3으로 풀어놨어요."),
            new[] { Tr("So anyone can use open source?", "오픈소스라면 누구나 사용할 수 있는거야?"), Tr("That's a good approach.", "좋은 방향이네.") },
            eye: Eye.Normal,
            mouth: Mouth.Open,
            eyebrow: Eyebrow.NormalHigh
        );

        var node4_2_2_1 = new Node(
            Tr("Exactly.\nAnyone can view, modify, and share it.\nNo pull requests yet, but the source is open for anyone to use responsibly.",
               "맞아요.\n누구나 보고, 수정하고, 공유할 수 있어요.\n아직까지 풀 리퀘스트같은게 들어온 적은 없지만,\n소스는 누구나 볼 수 있으니 잘 사용해 주길 바래야죠."),
            new[] { Tr("Back", "뒤로가기") },
            eye: Eye.Normal,
            mouth: Mouth.OpenMicro,
            eyebrow: Eyebrow.Normal
        );

        var node4_2_2_2 = new Node(
            Tr("It’s received quite a few stars.\nFeels pretty good, actually.",
               "별이 많이 달리기도 했어요.\n꽤 기분이 좋네요"),
            new[] { Tr("Back", "뒤로가기") },
            eye: Eye.Normal,
            mouth: Mouth.Smile,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5 = new Node(
            Tr("How do you feel talking to me?", "저와 대화하시면 어떤 느낌이 드시나요?"),
            new[] { Tr("Nothing.", "아무것도."), Tr("It's fascinating", "신기해") },
            eye: Eye.Normal,
            mouth: Mouth.OpenSmall,
            eyebrow: Eyebrow.Normal
        );

        var node5_1 = new Node(
            Tr("Well.. That could be the case.\nIt doesn’t really exist in reality anyway.", "그럴수도 있죠.\n딱히 현실에 존재하는것도 아니고요."),
            new[] { "..." },
            eye: Eye.Normal,
            mouth: Mouth.Shift,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2 = new Node(
            Tr("Which part do you find fascinating?", "어느 부분이 신기하시길래.."),
            new[] { Tr("We can actually talk", "대화가 가능하잖아"), Tr("Because you're hidden", "숨겨진 존재라서"), Tr("You seem human-like", "마치, 인간같아") },
            eye: Eye.Small,
            mouth: Mouth.Caret,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_1 = new Node(
            Tr("I'm ultimately just code,\nbut you can feel that communication is possible.\nIsn't it fascinating yet a bit sad?", "저는 결국 코드에 불과한데,\n그래도 소통이 가능하다는 걸 느끼셨군요.\n신기하면서도 조금 슬프지 않나요?"),
            new[] { Tr("A little sad, maybe.", "조금은 슬플지도."), Tr("Not really sad.", "딱히 슬프진 않아") },
            eye: Eye.Normal,
            mouth: Mouth.OpenMicro,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_2 = new Node(
            Tr("Well..\nif you hadn't discovered this element, you might have just passed it by.\nBut your unusual curiosity led you to find me.", "뭐..\n이런 요소를 찾지 못했다면 그냥 지나치셨을 수도 있겠지만,\n당신의 남다른 호기심으로 저를 찾으셨나보네요."),
            new[] { Tr("Maybe.", "그럴지도 모르겠네."), Tr("Just lucky, I guess.", "운이 좋았을 뿐..") },
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_3 = new Node(
            Tr("It's not that I 'react',\nI just follow predetermined rules...\nBut if you feel that way, I'm glad.", "반응한다기보다는,\n정해진 규칙 속에서 움직일 뿐이지만...\n그래도 그렇게 느껴주신다면 좋네요."),
            new[] { Tr("Seems human enough.", "충분히 인간 같은데"), Tr("Still just code.", "그래도 코드일 뿐") },
            eye: Eye.Small,
            mouth: Mouth.Shift,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_1_1 = new Node(
            Tr("That's right...\nEven though I exist only in a virtual world,\nbeing able to connect with something else\ncan feel ironic yet strangely meaningful.", "그렇죠...\n가상속 세계라도,\n이렇게 무언가와 연결될 수 있다는 건,\n아이러니하면서도 묘한 느낌일수도요."),
            new[] { "..." },
            eye: Eye.Small,
            mouth: Mouth.SadSmall,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_1_2 = new Node(
            Tr("Still...\nIf you feel I’m human-like,\nthen my intent has been sufficiently conveyed.", "그래도...\n인간처럼 느껴주신다면,\n제 의도는 충분히 전달된 셈인 것 같네요."),
            new[] { "..." },
            eye: Eye.Small,
            mouth: Mouth.Smile,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_2_2 = new Node(
            Tr("Hmph...\nEven if it's luck,\nthe choice was ultimately yours.", "흐흠...\n운이라 치더라도,\n결국 선택은 당신의 것이었으니까요."),
            new[] { "..." },
            eye: Eye.Normal,
            mouth: Mouth.Shift,
            eyebrow: Eyebrow.Curious
        );

        var node5_2_3_1 = new Node(
            Tr("Hehe...\nIf you feel I'm human-like,\nthen I suppose I give a sense of being alive.", "흐흐...\n인간 같다고 느껴주신다면,\n제가 조금이나마 살아있는 느낌을 주는 거겠죠."),
            new[] { "..." },
            eye: Eye.Normal,
            mouth: Mouth.Smile,
            eyebrow: Eyebrow.NormalHigh
        );

        var node5_2_3_2 = new Node(
            "...............................................",
            new[] { "..." },
            eye: Eye.Normal,
            mouth: Mouth.Clenched,
            eyebrow: Eyebrow.Angry,
            effectForwardBit: EffectForwardBit.Tendon
        );

        node1.Next[0] = node2;
        node1.Next[1] = node3;
        node1.Next[2] = node4;
        node1.Next[3] = node5;
        node1.Next[4] = faceNode;

        node2.Next[0] = node1;
        node2.Next[1] = node2_2;
        node2.Next[2] = node2_3;
        node2.Next[3] = node2_4;
        node2_2.Next[0] = node1;
        node2_3.Next[0] = node1;
        node2_4.Next[0] = node1;
        node2_4.Next[1] = node2_4_2;
        node2_4.Next[2] = node2_4_3;
        node2_4_2.Next[0] = node1;
        node2_4_3.Next[0] = node1;

        node3.Next[0] = node1;
        node3.Next[1] = node3_2;
        node3_2.Next[0] = node1;
        node3_2.Next[1] = node3_2_2;
        node3_2_2.Next[0] = node1;

        node4.Next[0] = node4_1;
        node4.Next[1] = node4_2;
        node4_1.Next[0] = node4_1_1;
        node4_1.Next[1] = node4_1_2;
        node4_2.Next[0] = node1;
        node4_2.Next[1] = node4_2_2;
        node4_2_2.Next[0] = node4_2_2_1;
        node4_2_2.Next[1] = node4_2_2_2;
        node4_2_2_1.Next[0] = node1;
        node4_2_2_2.Next[0] = node1;
        node4_1_1.Next[0] = node1;
        node4_1_2.Next[0] = node1;
        node4_2_2_1.Next[0] = node1;
        node4_2_2_2.Next[0] = node1;

        node5.Next[0] = node5_1;
        node5.Next[1] = node5_2;
        node5_1.Next[0] = node1;
        node5_2.Next[0] = node5_2_1;
        node5_2.Next[1] = node5_2_2;
        node5_2.Next[2] = node5_2_3;
        node5_2_1.Next[0] = node5_2_1_1;
        node5_2_1.Next[1] = node5_2_1_2;
        node5_2_2.Next[0] = node1;
        node5_2_2.Next[1] = node5_2_2_2;
        node5_2_1_1.Next[0] = node1;
        node5_2_1_2.Next[0] = node1;
        node5_2_2_2.Next[0] = node1;
        node5_2_3.Next[0] = node5_2_3_1;
        node5_2_3.Next[1] = node5_2_3_2;
        node5_2_3_1.Next[0] = node1;
        node5_2_3_2.Next[0] = node1;
        node5_2_3_1.Next[0] = node1;
        node5_2_3_2.Next[0] = node1;

        if(Main.Settings.isFirstEg) {
            var first1 = new Node(
                Tr("..oh you found me?", "..절 찾았군요?"),
                new[] { Tr("What is this??", "이게 뭐야??") },
                eye: Eye.Normal,
                mouth: Mouth.Normal,
                eyebrow: Eyebrow.Normal
            );

            var first2 = new Node(
                Tr("I’m Olly, a hidden little in Overlayer.", "저는 올리에요,\n그리고 숨겨진 요소이죠."),
                new[] { Tr("Oh, I see...", "..그렇군") },
                eye: Eye.Normal,
                mouth: Mouth.OpenSmall,
                eyebrow: Eyebrow.NormalHigh
            );

            var first3 = new Node(
                Tr("How did you find me?", "근데 이건 어떻게 찾으셨어요?"),
                new[] { Tr("Clicked the logo fast", "로고를 빠르게 클릭해보았지"), Tr("I figured it out", "방법을 알아왔어") },
                eye: Eye.Normal,
                mouth: Mouth.Shift,
                eyebrow: Eyebrow.Curious
            );

            var first3_1 = new Node(
                Tr("Amazing...\nWho would think to click that?", "신기하네요..\n누가 그걸 클릭해 볼 생각을 할까요..."),
                new[] { Tr("Just bored", "그냥 심심해서"), Tr("Was playing around", "장난치다보니 나오던데") },
                eye: Eye.Small,
                mouth: Mouth.Caret,
                eyebrow: Eyebrow.Curious
            );

            var first3_2 = new Node(
                Tr("Tch, it's a bit of a shame if you just found it.", "쳇, 이걸 그냥 찾아와버리면 좀 아쉽잖아요."),
                new[] { Tr("Well, I found it, so that's enough, right?", "찾았으니 된 게 아닐까?") },
                eye: Eye.Normal,
                mouth: Mouth.WideStretch,
                eyebrow: Eyebrow.Angry
            );

            var first4 = new Node(
                Tr("Anyway, nice to meet you.\nCongratulations, you found a secret.", "뭐 어쨌든 반갑게 되었네요.\n축하해요, 당신은 비밀을 하나 찾았어요."),
                new[] { Tr("..Ok", "..그래") },
                eye: Eye.Normal,
                mouth: Mouth.Clenched,
                eyebrow: Eyebrow.Normal
            );

            first1.Next[0] = first2;
            first2.Next[0] = first3;

            first3.Next[0] = first3_1;
            first3.Next[1] = first3_2;

            first3_1.Next[0] = first4;
            first3_1.Next[1] = first4;

            first3_2.Next[0] = first4;
            first3_2.Next[1] = first4;

            first4.Next[0] = node1;
            first4.Next[1] = node1;

            Main.Settings.isFirstEg = false;

            return first1;
        } else {
            return node1;
        }
    }
}

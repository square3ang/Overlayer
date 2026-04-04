using static Overlayer.Olly.OllyDialogue;
using static Overlayer.Olly.OllyState;
using static Overlayer.Olly.OllyUtils;

namespace Overlayer.Olly;

public partial class Olly {
    private Node MakeDialogue() {
        var node1 = new Node(
            "...",
            [Tr("About you", "너에 관해"), Tr("Overlayer", "오버레이어"), Tr("Source Code", "소스코드"), Tr("Face", "얼굴")],
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.Normal,
            onChoice: (num) => {
                if(num == 3) {
                    FollowMouse = true;
                }
            }
        );

        var faceNode = new Node(
            "..",
            [Tr("Back", "뒤로가기")],
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.Normal,
            onChoice: _ => FollowMouse = false);
        faceNode.Next[0] = node1;

        var node2 = new Node(
            Tr("I'm Olly.\nThe name comes from words used in Overlayer.", "저는 올리에요.\nOverlayer에 들어간 단어로부터 이름을 지었다고 하네요."),
            ["...", Tr("How did you come to be?", "어쩌다가 만들어졌어?"), Tr("Who drew you?", "널 누가 그려준거야?")],
            eye: Eye.Normal,
            mouth: Mouth.OpenSmall,
            eyebrow: Eyebrow.Normal
        );

        var node2_2 = new Node(
            Tr("They just added some interesting things they thought of along the way.", "적당히 넣을만한..\n신기한 것들을 생각하다가 넣으셨다고 전하시라네요."),
            ["..."],
            eye: Eye.Normal,
            mouth: Mouth.Clenched,
            eyebrow: Eyebrow.NormalHigh
        );

        var node2_3 = new Node(
            Tr("Kkitut drew me.", "Kkitut님이 저를 그려주셨어요."),
            ["..."],
            eye: Eye.Normal,
            mouth: Mouth.Normal,
            eyebrow: Eyebrow.Normal
        );

        var node3 = new Node(
            Tr("Originally it was a mod made by c3nb,\nbut now mostly Kkitut is in charge.\nThey had a hard time rewriting c3nb's code.", "처음엔 c3nb가 만든 모드였지만,\n이제는 대부분 Kkitut이 맞고 있어요.\nc3nb의 코드를 갈아치우느라 고생이 많았다고 했죠."),
            ["...", Tr("Hard time?", "고생?")],
            eye: Eye.Normal,
            mouth: Mouth.Open,
            eyebrow: Eyebrow.Normal
        );

        var node3_2 = new Node(
            Tr("You may not know this, but the previous Overlayer was full of ads...\nIt was quite inconvenient in many ways.", "아실 진 모르겠지만..\n이전 오버레이어는 광고 투성이에...\n여러모로 불편하기 짝이 없었거든요."),
            ["...", Tr("So..?", "그래서..?")],
            eye: Eye.Small,
            mouth: Mouth.OpenDisgust,
            eyebrow: Eyebrow.Sad
        );

        var node3_2_2 = new Node(
            Tr("So they spent quite a lot of time rewriting it from scratch.", "그래서 그걸 갈아엎느라 좀 시간을 많이 쓰셨다고 한다네요."),
            ["..."],
            eye: Eye.Normal,
            mouth: Mouth.Shift,
            eyebrow: Eyebrow.NormalHigh
        );

        var node4 = new Node(
            Tr("Overlayer is an open-source mod released under GPL v3.",
               "오버레이어는 GPL v3으로 공개되어 있는 오픈 소스 모드예요."),
            ["...", Tr("What does that mean?", "그게 어떤 의미야?"), Tr("How can I help?", "어떻게 도울 수 있어?")],
            eye: Eye.Normal,
            mouth: Mouth.Open,
            eyebrow: Eyebrow.Normal
        );

        var node4_2 = new Node(
            Tr("It means anyone can view, modify, and share the code.\nHowever, any derivative work must also be open under the same license.",
               "누구나 코드를 보고, 수정하고, 공유할 수 있다는 뜻이죠.\n단, 이를 수정한 작업물도 반드시 같은 라이선스로 공개해야 해요."),
            ["..."],
            eye: Eye.Normal,
            mouth: Mouth.OpenSmallHarf,
            eyebrow: Eyebrow.NormalHigh
        );

        var node4_3 = new Node(
            Tr("You can help by reporting bugs or suggesting new features.\nIf you're a developer, feel free to submit a Pull Request.",
               "버그를 제보하거나 새로운 기능을 제안하며 도와줄 수 있어요.\n개발자라면 직접 코드를 수정해서 풀 리퀘스트를 보내주셔도 좋아요."),
            ["...", Tr("Where is the source?", "소스 코드는 어디에 있어?")],
            eye: Eye.Normal,
            mouth: Mouth.Smile,
            eyebrow: Eyebrow.Normal
        );

        var node4_3_1 = new Node(
            Tr("The source code is hosted on GitHub.\nI'm always waiting for someone to help improve Overlayer.",
               "소스 코드는 GitHub에 올라와 있어요.\n오버레이어를 함께 더 좋게 만들어줄 분을 언제나 기다리고 있어요."),
            ["..."],
            eye: Eye.Normal,
            mouth: Mouth.OpenMicro,
            eyebrow: Eyebrow.NormalHigh
        );

        node1.Next[0] = node2;
        node1.Next[1] = node3;
        node1.Next[2] = node4;
        node1.Next[3] = faceNode;

        node2.Next[0] = node1;
        node2.Next[1] = node2_2;
        node2.Next[2] = node2_3;
        node2_2.Next[0] = node1;
        node2_3.Next[0] = node1;
        node2_3.Next[0] = node1;

        node3.Next[0] = node1;
        node3.Next[1] = node3_2;
        node3_2.Next[0] = node1;
        node3_2.Next[1] = node3_2_2;
        node3_2_2.Next[0] = node1;

        node4.Next[0] = node1;
        node4.Next[1] = node4_2;
        node4.Next[2] = node4_3;
        node4_2.Next[0] = node1;
        node4_3.Next[0] = node1;
        node4_3.Next[1] = node4_3_1;
        node4_3_1.Next[0] = node1;

        if(Main.Settings.IsFirstEg) {
            var first1 = new Node(
                Tr("..oh you found me?", "..절 찾았군요?"),
                [Tr("What is this??", "이게 뭐야??")],
                eye: Eye.Normal,
                mouth: Mouth.Normal,
                eyebrow: Eyebrow.Normal
            );

            var first2 = new Node(
                Tr("I’m Olly, a hidden little in Overlayer.", "저는 올리에요,\n그리고 숨겨진 요소이죠."),
                [Tr("Oh, I see...", "..그렇군")],
                eye: Eye.Normal,
                mouth: Mouth.OpenSmall,
                eyebrow: Eyebrow.NormalHigh
            );

            var first3 = new Node(
                Tr("How did you find me?", "근데 이건 어떻게 찾으셨어요?"),
                [Tr("Clicked the logo fast", "로고를 빠르게 클릭해보았지"), Tr("I figured it out", "방법을 알아왔어")],
                eye: Eye.Normal,
                mouth: Mouth.Shift,
                eyebrow: Eyebrow.Curious
            );

            var first3_1 = new Node(
                Tr("Amazing...\nWho would think to click that?", "신기하네요..\n누가 그걸 클릭해 볼 생각을 할까요..."),
                [Tr("Just bored", "그냥 심심해서"), Tr("Was playing around", "장난치다보니 나오던데")],
                eye: Eye.Small,
                mouth: Mouth.Caret,
                eyebrow: Eyebrow.Curious
            );

            var first3_2 = new Node(
                Tr("Tch, it's a bit of a shame if you just found it.", "쳇, 이걸 그냥 찾아와버리면 좀 아쉽잖아요."),
                [Tr("Well, I found it, so that's enough, right?", "찾았으니 된 게 아닐까?")],
                eye: Eye.Normal,
                mouth: Mouth.WideStretch,
                eyebrow: Eyebrow.Angry
            );

            var first4 = new Node(
                Tr("Anyway, nice to meet you.\nCongratulations, you found a secret.", "뭐 어쨌든 반갑게 되었네요.\n축하해요, 당신은 비밀을 하나 찾았어요."),
                [Tr("..Ok", "..그래")],
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

            Main.Settings.IsFirstEg = false;

            return first1;
        } else {
            return node1;
        }
    }
}

# Adaptive-Formation-Control-and-Transformation-in-Virtual-Crowds-via-Deep-Reinforcement-Learning
This paper proposes a novel method for adaptive formation control and transformation in virtual crowds, based on deep reinforcement learning. This method enables virtual agents to autonomously generate, maintain, and adaptively transform their formations without external guidance, relying solely on local perception and interaction.

The proposed framework comprises three core components:
1. Dynamic formation configuration mechanism based on alignment.
2. Formation control algorithm with embedded formation constraints.
3. Adaptive formation transformation mechanism.

---

## Requirements
**Python=3.10.12，mlagent=1.1.0，Pytorch, onnx, protobuf, tensorboard and so on.**

* The requirements.txt file lists all required libraries. Install them by following these steps:
1. Create a conda environment:
```
conda create -n your_env python==3.10.12  
```
2. Install Unity ML-Agents Toolkit following the [official installation instructions](https://github.com/Unity-Technologies/ml-agents)
3. Install PyTorch following the [official installation instructions](https://pytorch.org/)
4. Install remaining packages via pip:
```
pip install package_name
```

## Train
1. **Build Unity Executable**  
   - Set up the agent and environment configurations in the Unity Editor, or directly import `myproject.unitypackage` to obtain the C# scripts related to this paper, along with preconfigured settings for the agent(s) and environment. Then export as Linux `.x86_64` or Windows `.exe`.
2. **Configure Training**  
   - Place and configure the training YAML `training_config.yaml` in the build directory.
3. **Launch Training**  
   ```
   cd ./build_directory
   conda activate your_env
   mlagents-learn training_config.yaml --run-id=your_id --env=project_name.x86_64 --num-envs=16 --no-graphics
   ```
4. **Resume training after interruption**
   ```
   mlagents-learn training_config.yaml --run-id=your_id --env=project_name.x86_64 --num-envs=16 --no-graphics --resume
   ```
   
## Example
![example](https://github.com/qyc15180240677/Adaptive-Formation-Control-and-Transformation-in-Virtual-Crowds-via-Deep-Reinforcement-Learning/blob/main/output.gif "6 agents form a circular formation")
